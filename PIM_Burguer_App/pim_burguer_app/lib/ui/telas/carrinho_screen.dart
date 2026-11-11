import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/app_estado.dart';
import '../../core/formato.dart';
import '../../core/rotas.dart';
import '../widgets/componentes.dart';

class CarrinhoScreen extends StatelessWidget {
  const CarrinhoScreen({super.key});

  Future<void> _checkout(BuildContext context) async {
    final estado = context.read<AppEstado>();
    if (estado.usuario == null) {
      await Navigator.pushNamed(context, Rotas.login);
    }
    if (context.mounted && estado.usuario != null) {
      await Navigator.pushNamed(context, Rotas.checkout);
    }
  }

  @override
  Widget build(BuildContext context) {
    final estado = context.watch<AppEstado>();
    final itens = estado.carrinho;
    return Scaffold(
      appBar: AppBar(title: const Text('Meu carrinho')),
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            if (itens.isEmpty)
              Mensagem(
                Icons.shopping_bag_outlined,
                'Seu carrinho está vazio',
                texto: 'Escolha o que vai no seu próximo pedido.',
                acao: FilledButton(
                  onPressed: () => Navigator.pop(context),
                  child: const Text('Ver cardápio'),
                ),
              ),
            for (final item in itens.entries)
              Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: _ItemCarrinho(item.key, item.value),
              ),
            if (itens.isNotEmpty) ...[
              if (estado.indisponiveis)
                const Aviso(
                  'Atualize o cardápio ou remova os itens indisponíveis antes de fazer um novo pedido.',
                ),
              const SizedBox(height: 12),
              Card(
                child: ListTile(
                  title: const Text('Subtotal estimado'),
                  subtitle: const Text(
                    'Entrega e descontos são calculados na revisão.',
                  ),
                  trailing: Text(
                    moeda(estado.subtotalCentavos / 100),
                    style: const TextStyle(fontWeight: FontWeight.bold),
                  ),
                ),
              ),
              const SizedBox(height: 14),
              FilledButton(
                onPressed: estado.gravando ? null : () => _checkout(context),
                child: const Text('Continuar para checkout'),
              ),
            ],
            if (itens.isEmpty && estado.usuario != null)
              TextButton(
                onPressed: () => _checkout(context),
                child: const Text('Verificar envio pendente'),
              ),
          ],
        ),
      ),
    );
  }
}

class _ItemCarrinho extends StatelessWidget {
  final int id;
  final int quantidade;
  const _ItemCarrinho(this.id, this.quantidade);

  @override
  Widget build(BuildContext context) {
    final estado = context.watch<AppEstado>();
    final produto = estado.produto(id);
    Future<void> mudar(int nova) async {
      try {
        await estado.mudarQuantidade(id, nova);
      } catch (erro) {
        if (context.mounted) avisar(context, mensagemErro(erro));
      }
    }

    return Card(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              produto?.nome ?? 'Produto #$id',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            Text(
              produto == null
                  ? 'Indisponível'
                  : moeda(produto.precoCentavos * quantidade / 100),
            ),
            Row(
              children: [
                IconButton(
                  tooltip: quantidade == 1 ? 'Remover' : 'Diminuir quantidade',
                  onPressed:
                      estado.gravando || (produto == null && quantidade > 1)
                      ? null
                      : () => mudar(quantidade - 1),
                  icon: const Icon(Icons.remove_circle_outline),
                ),
                Text('$quantidade', semanticsLabel: '$quantidade unidades'),
                IconButton(
                  tooltip: 'Aumentar quantidade',
                  onPressed:
                      estado.gravando ||
                          produto == null ||
                          quantidade >= 20 ||
                          estado.unidades >= 100
                      ? null
                      : () => mudar(quantidade + 1),
                  icon: const Icon(Icons.add_circle_outline),
                ),
                const Spacer(),
                IconButton(
                  tooltip: 'Remover produto',
                  onPressed: estado.gravando ? null : () => mudar(0),
                  icon: const Icon(Icons.delete_outline),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
