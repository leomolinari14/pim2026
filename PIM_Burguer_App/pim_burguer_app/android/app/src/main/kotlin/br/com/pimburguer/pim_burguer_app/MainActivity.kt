package br.com.pimburguer.pim_burguer_app

import android.content.ContentValues
import android.content.Context
import android.content.Intent
import android.database.sqlite.SQLiteDatabase
import android.database.sqlite.SQLiteOpenHelper
import android.net.Uri
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import io.flutter.embedding.android.FlutterActivity
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.plugin.common.MethodChannel
import java.security.KeyStore
import java.security.MessageDigest
import java.util.concurrent.Executors
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

class MainActivity : FlutterActivity() {
    private val executor = Executors.newSingleThreadExecutor()
    private var banco: BancoLocal? = null
    private var origem: String? = null

    override fun configureFlutterEngine(engine: FlutterEngine) {
        super.configureFlutterEngine(engine)
        MethodChannel(engine.dartExecutor.binaryMessenger, "pim_burguer/navegacao")
            .setMethodCallHandler { call, result ->
                if (call.method != "abrirWhatsApp") {
                    result.notImplemented()
                } else {
                    try {
                        val uri = Uri.parse(requireNotNull(call.argument<String>("url")))
                        require(uri.scheme == "https" && uri.host in setOf("wa.me", "api.whatsapp.com", "www.whatsapp.com"))
                        startActivity(Intent(Intent.ACTION_VIEW, uri))
                        result.success(null)
                    } catch (_: Exception) {
                        result.error("whatsapp", "Não foi possível abrir o WhatsApp.", null)
                    }
                }
            }
        val canal = MethodChannel(engine.dartExecutor.binaryMessenger, "pim_burguer/armazenamento")
        val pendencias = Pendencias(applicationContext)
        canal.setMethodCallHandler { call, result ->
            executor.execute {
                try {
                    val api = requireNotNull(call.argument<String>("origem"))
                    val conta = "$api|" + call.argument<String>("usuario")
                    val resposta = when (call.method) {
                        "lerPendencia" -> pendencias.ler(conta)
                        "salvarPendencia" -> pendencias.salvar(conta, requireNotNull(call.argument("json")))
                        "excluirPendencia" -> pendencias.excluir(conta)
                        else -> {
                            val db = if (origem == api) banco!! else {
                                banco?.close()
                                BancoLocal(applicationContext, api).also {
                                    banco = it
                                    origem = api
                                }
                            }
                            when (call.method) {
                                "lerProdutos" -> db.produtos()
                                "salvarProdutos" -> db.salvarProdutos(requireNotNull(call.argument("produtos")))
                                "lerCarrinho" -> db.carrinho()
                                "salvarCarrinho" -> db.salvarCarrinho(requireNotNull(call.argument("itens")))
                                else -> {
                                    runOnUiThread { result.notImplemented() }
                                    return@execute
                                }
                            }
                        }
                    }
                    runOnUiThread { result.success(if (resposta is Unit) null else resposta) }
                } catch (_: Exception) {
                    runOnUiThread { result.error("armazenamento", "Não foi possível acessar os dados do aparelho.", null) }
                }
            }
        }
    }

    override fun onDestroy() {
        executor.execute { banco?.close() }
        executor.shutdown()
        super.onDestroy()
    }
}

private fun identificador(texto: String): String = MessageDigest.getInstance("SHA-256")
    .digest(texto.toByteArray()).joinToString("") { "%02x".format(it) }

private class BancoLocal(context: Context, api: String) :
    SQLiteOpenHelper(context, "pim_burguer_" + identificador(api).take(16) + ".db", null, 1) {

    override fun onCreate(db: SQLiteDatabase) {
        db.execSQL("""CREATE TABLE produtos (
            id INTEGER PRIMARY KEY, nome TEXT NOT NULL, descricao TEXT NOT NULL,
            preco_centavos INTEGER NOT NULL, categoria TEXT NOT NULL, imagem TEXT NOT NULL
        )""")
        db.execSQL("""CREATE TABLE carrinho (
            produto_id INTEGER PRIMARY KEY, quantidade INTEGER NOT NULL CHECK (quantidade BETWEEN 1 AND 20)
        )""")
    }

    override fun onUpgrade(db: SQLiteDatabase, oldVersion: Int, newVersion: Int) = Unit

    fun produtos(): List<Map<String, Any>> = readableDatabase.rawQuery(
        "SELECT id, nome, descricao, preco_centavos, categoria, imagem FROM produtos ORDER BY id", null
    ).use { cursor ->
        buildList {
            while (cursor.moveToNext()) add(mapOf(
                "id" to cursor.getInt(0), "nome" to cursor.getString(1), "descricao" to cursor.getString(2),
                "precoCentavos" to cursor.getLong(3), "categoria" to cursor.getString(4), "imagem" to cursor.getString(5)
            ))
        }
    }

    fun carrinho(): Map<Int, Int> = readableDatabase.rawQuery(
        "SELECT produto_id, quantidade FROM carrinho ORDER BY produto_id", null
    ).use { cursor -> buildMap { while (cursor.moveToNext()) put(cursor.getInt(0), cursor.getInt(1)) } }

    fun salvarProdutos(produtos: List<Map<String, Any>>) = substituir("produtos") { db ->
        for (produto in produtos) db.insertOrThrow("produtos", null, ContentValues().apply {
            put("id", (produto.getValue("id") as Number).toInt())
            put("nome", produto.getValue("nome") as String)
            put("descricao", produto.getValue("descricao") as String)
            put("preco_centavos", (produto.getValue("precoCentavos") as Number).toLong())
            put("categoria", produto.getValue("categoria") as String)
            put("imagem", produto.getValue("imagem") as String)
        })
    }

    fun salvarCarrinho(itens: Map<Int, Int>) = substituir("carrinho") { db ->
        for ((id, quantidade) in itens) db.insertOrThrow("carrinho", null, ContentValues().apply {
            put("produto_id", id)
            put("quantidade", quantidade)
        })
    }

    private fun substituir(tabela: String, inserir: (SQLiteDatabase) -> Unit) {
        val db = writableDatabase
        db.beginTransaction()
        try {
            db.delete(tabela, null, null)
            inserir(db)
            db.setTransactionSuccessful()
        } finally {
            db.endTransaction()
        }
    }
}

private class Pendencias(context: Context) {
    private val dados = context.getSharedPreferences("checkout_pim", Context.MODE_PRIVATE)
    private val alias = "pim_burguer_checkout_v1"

    private fun chave(): SecretKey {
        val keystore = KeyStore.getInstance("AndroidKeyStore").apply { load(null) }
        val existente = keystore.getKey(alias, null) as? SecretKey
        if (existente != null) return existente
        return KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, "AndroidKeyStore").apply {
            init(KeyGenParameterSpec.Builder(alias, KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE).build())
        }.generateKey()
    }

    fun ler(conta: String): String? {
        val registro = dados.getString(identificador(conta), null) ?: return null
        val bytes = Base64.decode(registro, Base64.NO_WRAP)
        val cipher = Cipher.getInstance("AES/GCM/NoPadding")
        cipher.init(Cipher.DECRYPT_MODE, chave(), GCMParameterSpec(128, bytes.copyOfRange(0, 12)))
        cipher.updateAAD(conta.toByteArray())
        return cipher.doFinal(bytes.copyOfRange(12, bytes.size)).toString(Charsets.UTF_8)
    }

    fun salvar(conta: String, json: String) {
        val cipher = Cipher.getInstance("AES/GCM/NoPadding")
        cipher.init(Cipher.ENCRYPT_MODE, chave())
        cipher.updateAAD(conta.toByteArray())
        val bytes = cipher.iv + cipher.doFinal(json.toByteArray())
        check(dados.edit().putString(identificador(conta), Base64.encodeToString(bytes, Base64.NO_WRAP)).commit())
    }

    fun excluir(conta: String) {
        check(dados.edit().remove(identificador(conta)).commit())
    }
}
