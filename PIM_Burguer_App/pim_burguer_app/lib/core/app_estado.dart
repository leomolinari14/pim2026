import 'dart:async';

import 'package:flutter/foundation.dart';

import '../data/api/api_cliente.dart';
import '../data/api/conta_api.dart';
import '../data/db/app_database.dart';
import '../data/repositories/pedido_repository.dart';
import '../data/repositories/produto_repository.dart';
import '../models/pedido.dart';
import '../models/produto.dart';
import '../models/usuario.dart';

class AppEstado extends ChangeNotifier {
  final ApiCliente api;
  final ContaApi conta;
  final ProdutoRepository catalogo;
  final PedidoRepository pedidos;
  final AppDatabase banco;

  AppEstado(this.api, this.conta, this.catalogo, this.pedidos, this.banco);

  Map<int, Produto> _produtos = {};
  Map<int, int> _carrinho = {};
  Sessao? _sessao;
  Timer? _expiracao;
  bool _gravando = false;
  bool _enviando = false;
  bool _encerrado = false;

  bool inicializando = true;
  bool atualizando = false;
  String? avisoCatalogo;
  String? erroLocal;

  Usuario? get usuario => _sessao?.usuario;
  List<Produto> get produtos => _produtos.values.toList();
  Produto? produto(int id) => _produtos[id];
  Map<int, int> get carrinho => Map.unmodifiable(_carrinho);
  int get unidades => _carrinho.values.fold(0, (a, b) => a + b);
  int get subtotalCentavos => _carrinho.entries.fold(
    0,
    (soma, item) =>
        soma + (_produtos[item.key]?.precoCentavos ?? 0) * item.value,
  );
  bool get indisponiveis =>
      _carrinho.keys.any((id) => !_produtos.containsKey(id));
  bool get gravando => _gravando || _enviando;

  Future<void> iniciar() async {
    erroLocal = null;
    inicializando = true;
    _notificar();
    try {
      final cache = await catalogo.listarLocal();
      _produtos = {for (final produto in cache) produto.id: produto};
      _carrinho = await banco.lerCarrinho();
    } catch (_) {
      erroLocal = 'Não foi possível abrir o banco local. Tente novamente.';
    } finally {
      inicializando = false;
      _notificar();
    }
    if (erroLocal == null) await atualizarCatalogo();
  }

  Future<void> atualizarCatalogo() async {
    if (atualizando) return;
    atualizando = true;
    avisoCatalogo = null;
    _notificar();
    try {
      final itens = await catalogo.atualizar();
      _produtos = {for (final produto in itens) produto.id: produto};
    } catch (erro) {
      avisoCatalogo = erro is ApiErro
          ? erro.mensagem
          : 'Não foi possível atualizar o cardápio.';
    } finally {
      atualizando = false;
      _notificar();
    }
  }

  Future<void> mudarQuantidade(int id, int quantidade) async {
    if (gravando) return;
    final itens = {..._carrinho};
    if (quantidade <= 0) {
      itens.remove(id);
    } else {
      if (!_produtos.containsKey(id)) {
        throw const ApiErro('Este produto ficou indisponível.');
      }
      itens[id] = quantidade;
    }
    await _salvarCarrinho(itens);
  }

  Future<void> _salvarCarrinho(Map<int, int> itens) async {
    if (itens.length > 30 ||
        itens.values.any((n) => n < 1 || n > 20) ||
        itens.values.fold(0, (a, b) => a + b) > 100) {
      throw const ApiErro(
        'Limites do carrinho: 30 produtos distintos, 20 por produto e 100 unidades no total.',
      );
    }
    _gravando = true;
    _notificar();
    try {
      await banco.salvarCarrinho(itens);
      _carrinho = itens;
    } finally {
      _gravando = false;
      _notificar();
    }
  }

  void _definirSessao(Sessao? sessao) {
    _expiracao?.cancel();
    _sessao = sessao;
    if (sessao != null) {
      _expiracao = Timer(
        sessao.expiraEm.difference(DateTime.now()),
        () => _definirSessao(null),
      );
    }
    _notificar();
  }

  Future<void> autenticar(bool cadastrar, Map<String, Object?> dados) async {
    final sessao = await conta.autenticar(cadastrar, dados);
    if (sessao.usuario.perfil != 'Cliente') {
      try {
        await conta.sair(sessao.token);
      } catch (_) {}
      throw const ApiErro('Use uma conta de cliente para acessar o app.');
    }
    _definirSessao(sessao);
  }

  Future<void> sair() async {
    final sessao = _sessao;
    _definirSessao(null);
    if (sessao == null) return;
    try {
      await conta.sair(sessao.token);
    } on ApiErro catch (erro) {
      if (erro.status != 401) {
        throw const ApiErro(
          'Você saiu neste aparelho. A API não pôde confirmar o encerramento da sessão.',
        );
      }
    }
  }

  Sessao _exigirSessao() {
    final sessao = _sessao;
    if (sessao != null && sessao.expiraEm.isAfter(DateTime.now())) {
      return sessao;
    }
    throw const ApiErro('Entre novamente para continuar.', 401);
  }

  Future<T> _autorizado<T>(Future<T> Function(Sessao) acao) async {
    final sessao = _exigirSessao();
    try {
      return await acao(sessao);
    } on ApiErro catch (erro) {
      if (erro.status == 401 && _sessao?.token == sessao.token) {
        _definirSessao(null);
      }
      rethrow;
    }
  }

  Future<void> atualizarPerfil(String nome, String telefone) async {
    await _autorizado((sessao) async {
      final novo = await conta.atualizarPerfil(sessao.token, nome, telefone);
      if (_sessao?.token == sessao.token) {
        _definirSessao(Sessao(novo, sessao.token, sessao.expiraEm));
      }
    });
  }

  Future<void> alterarSenha(Map<String, Object?> dados) async {
    await _autorizado((sessao) async {
      await conta.alterarSenha(sessao.token, dados);
      if (_sessao?.token == sessao.token) _definirSessao(null);
    });
  }

  Future<PaginaPedidos> historico(int pagina) =>
      _autorizado((s) => pedidos.api.listar(s.token, pagina));

  Future<Pedido> detalhe(String id) =>
      _autorizado((s) => pedidos.api.detalhar(s.token, id));

  Future<List<Cupom>> cupons() =>
      _autorizado((s) => pedidos.api.cupons(s.token));

  Future<Cupom> adicionarCupom(String codigo) =>
      _autorizado((s) => pedidos.api.adicionarCupom(s.token, codigo));

  Future<RepeticaoPedido> repetir(String id) async {
    final repeticao = await _autorizado(
      (s) => pedidos.api.repetir(s.token, id),
    );
    if (repeticao.itens.isNotEmpty) {
      await atualizarCatalogo();
      await _salvarCarrinho(repeticao.itens);
    }
    return repeticao;
  }

  Future<Orcamento> orcar(Map<String, Object?> dados) =>
      _autorizado((s) => pedidos.api.orcar(s.token, dados));

  Future<bool> temPendencia() async =>
      await pedidos.pendencia(_exigirSessao().usuario.id) != null;

  Future<Pedido> confirmar([Map<String, Object?>? dados]) async {
    if (gravando) throw const ApiErro('Aguarde o envio em andamento.');
    _enviando = true;
    _notificar();
    try {
      return await _autorizado((sessao) async {
        final id = sessao.usuario.id;
        final pendente = await pedidos.pendencia(id);
        if (pendente != null && dados != null) {
          throw const ApiErro(
            'Resolva o pedido pendente antes de enviar outro.',
          );
        }
        final payload =
            pendente ??
            await pedidos.preparar(
              id,
              dados ?? (throw const ApiErro('Nenhum pedido pendente.')),
            );
        final Pedido pedido;
        try {
          pedido = await pedidos.api.enviar(sessao.token, payload);
        } on ApiErro catch (erro) {
          if (const {400, 409, 422}.contains(erro.status)) {
            await pedidos.concluir(id);
          }
          rethrow;
        }
        if (mapEquals(_carrinho, quantidades(payload['itens']))) {
          await _salvarCarrinho({});
        }
        await pedidos.concluir(id);
        return pedido;
      });
    } finally {
      _enviando = false;
      _notificar();
    }
  }

  void _notificar() {
    if (!_encerrado) notifyListeners();
  }

  @override
  void dispose() {
    _encerrado = true;
    _expiracao?.cancel();
    api.fechar();
    super.dispose();
  }
}
