import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/app_estado.dart';
import '../../core/formato.dart';
import '../../core/tema.dart';
import '../../models/produto.dart';
import '../widgets/componentes.dart';

class CatalogoScreen extends StatefulWidget {
  const CatalogoScreen({super.key});

  @override
  State<CatalogoScreen> createState() => _CatalogoScreenState();
}

class _CatalogoScreenState extends State<CatalogoScreen> {
  String? _categoria;
  String _busca = '';

  @override
  Widget build(BuildContext context) {
    final estado = context.watch<AppEstado>();
    if (estado.inicializando) {
      return const Center(child: CircularProgressIndicator());
    }
    if (estado.erroLocal != null) {
      return Center(
        child: Mensagem(
          Icons.storage,
          'Banco local indisponível',
          texto: estado.erroLocal,
          acao: FilledButton(
            onPressed: estado.iniciar,
            child: const Text('Tentar novamente'),
          ),
        ),
      );
    }
    final categorias = estado.produtos.map((p) => p.categoria).toSet().toList()
      ..sort();
    final ativa = categorias.contains(_categoria) ? _categoria : null;
    final produtos = estado.produtos.where((produto) {
      final texto = '${produto.nome} ${produto.descricao} ${produto.categoria}'
          .toLowerCase();
      return (ativa == null || produto.categoria == ativa) &&
          texto.contains(_busca);
    }).toList();
    return RefreshIndicator(
      onRefresh: estado.atualizarCatalogo,
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
        children: [
          const _Destaque(),
          const SizedBox(height: 18),
          Text(
            'Cardápio da loja',
            style: Theme.of(context).textTheme.headlineSmall
                ?.copyWith(fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 12),
          if (estado.atualizando) const LinearProgressIndicator(),
          if (estado.avisoCatalogo != null) ...[
            Aviso(
              estado.produtos.isEmpty
                  ? estado.avisoCatalogo!
                  : 'Exibindo o último cardápio salvo. ${estado.avisoCatalogo}',
            ),
            Align(
              alignment: Alignment.centerLeft,
              child: TextButton(
                onPressed: estado.atualizarCatalogo,
                child: const Text('Atualizar cardápio'),
              ),
            ),
          ],
          TextField(
            onChanged: (valor) =>
                setState(() => _busca = valor.trim().toLowerCase()),
            decoration: const InputDecoration(
              labelText: 'Buscar no cardápio',
              prefixIcon: Icon(Icons.search),
            ),
          ),
          if (categorias.isNotEmpty)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 10),
              child: Wrap(
                spacing: 8,
                runSpacing: 2,
                children: [
                  FilterChip(
                    label: const Text('Todos'),
                    selected: ativa == null,
                    onSelected: (_) => setState(() => _categoria = null),
                  ),
                  for (final categoria in categorias)
                    FilterChip(
                      label: Text(categoria),
                      selected: ativa == categoria,
                      onSelected: (valor) =>
                          setState(() => _categoria = valor ? categoria : null),
                    ),
                ],
              ),
            ),
          if (produtos.isEmpty &&
              !estado.atualizando &&
              estado.avisoCatalogo == null)
            const Mensagem(Icons.search_off, 'Nenhum produto encontrado'),
          for (final produto in produtos)
            Padding(
              padding: const EdgeInsets.only(bottom: 10),
              child: _ProdutoCard(produto),
            ),
        ],
      ),
    );
  }
}

class _Destaque extends StatelessWidget {
  const _Destaque();

  @override
  Widget build(BuildContext context) => ClipRRect(
    borderRadius: BorderRadius.circular(20),
    child: ColoredBox(
      color: verde,
      child: Row(
        children: [
          Expanded(
            flex: 3,
            child: Padding(
              padding: const EdgeInsets.fromLTRB(18, 23, 4, 23),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'HAMBURGUERIA',
                    style: TextStyle(
                      color: dourado,
                      fontWeight: FontWeight.w800,
                      letterSpacing: 1.4,
                      fontSize: 11,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Seu burger,\ndo seu jeito.',
                    style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                      color: Colors.white,
                      fontWeight: FontWeight.w800,
                      height: 1.08,
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'Peça para entregar ou retirar.',
                    style: TextStyle(color: Color(0xFFE0E4DB)),
                  ),
                ],
              ),
            ),
          ),
          Flexible(
            flex: 2,
            child: Image.asset(
              'assets/imagens/hero-hamburguer.webp',
              height: 148,
              width: 180,
              fit: BoxFit.contain,
              excludeFromSemantics: true,
            ),
          ),
        ],
      ),
    ),
  );
}

class _ProdutoCard extends StatelessWidget {
  final Produto produto;
  const _ProdutoCard(this.produto);

  @override
  Widget build(BuildContext context) {
    final estado = context.watch<AppEstado>();
    final imagem = produto.imagem;
    final quantidade = estado.carrinho[produto.id] ?? 0;
    final texto = Theme.of(context).textTheme;
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                ClipRRect(
                  borderRadius: BorderRadius.circular(12),
                  child: _foto(estado, imagem),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        produto.nome,
                        style: texto.titleMedium?.copyWith(
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                      const SizedBox(height: 3),
                      Text(produto.descricao),
                      const SizedBox(height: 6),
                      Text(
                        moeda(produto.precoCentavos / 100),
                        style: texto.titleMedium?.copyWith(
                          fontWeight: FontWeight.w800,
                          color: marca,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 6),
            Align(
              alignment: Alignment.centerRight,
              child: FilledButton.tonalIcon(
                onPressed:
                    estado.gravando ||
                        quantidade >= 20 ||
                        estado.unidades >= 100
                    ? null
                    : () async {
                        try {
                          await estado.mudarQuantidade(
                            produto.id,
                            quantidade + 1,
                          );
                          if (context.mounted) {
                            avisar(context, '${produto.nome} no carrinho.');
                          }
                        } catch (erro) {
                          if (context.mounted) {
                            avisar(context, mensagemErro(erro));
                          }
                        }
                      },
                icon: const Icon(Icons.add),
                label: Text(
                  quantidade == 0
                      ? 'Adicionar'
                      : 'Adicionar · $quantidade no carrinho',
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _foto(AppEstado estado, String imagem) {
    const largura = 88.0;
    const altura = 82.0;
    const vazio = ColoredBox(
      color: Color(0xFFF3E9D9),
      child: SizedBox(
        width: largura,
        height: altura,
        child: Icon(Icons.lunch_dining, color: marca),
      ),
    );
    const locais = {
      'produto-bacon',
      'produto-batata',
      'produto-classico',
      'produto-duplo',
      'produto-refrigerante',
      'produto-suco',
    };
    final arquivo = imagem.split('/').last.replaceFirst(RegExp(r'\.png$'), '');
    if (locais.contains(arquivo)) {
      return Image.asset(
        'assets/imagens/$arquivo.webp',
        width: largura,
        height: altura,
        fit: BoxFit.cover,
        excludeFromSemantics: true,
        errorBuilder: (_, _, _) => vazio,
      );
    }
    if (RegExp(r'^/?(?:midia/produtos|api/imagens)/[a-zA-Z0-9_.-]+$')
        .hasMatch(imagem)) {
      final caminho = imagem.startsWith('/') ? imagem.substring(1) : imagem;
      return Image.network(
        estado.api.base.resolve(caminho).toString(),
        width: largura,
        height: altura,
        fit: BoxFit.cover,
        excludeFromSemantics: true,
        errorBuilder: (_, _, _) => vazio,
      );
    }
    return vazio;
  }
}
