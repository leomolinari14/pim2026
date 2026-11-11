import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/app_estado.dart';
import '../../core/formato.dart';
import '../../core/rotas.dart';
import '../../models/pedido.dart';
import '../widgets/componentes.dart';

class PedidosScreen extends StatefulWidget {
  const PedidosScreen({super.key});

  @override
  State<PedidosScreen> createState() => _PedidosScreenState();
}

class _PedidosScreenState extends State<PedidosScreen> {
  late Future<PaginaPedidos> _resultado;
  int _pagina = 1;

  @override
  void initState() {
    super.initState();
    _resultado = context.read<AppEstado>().historico(_pagina);
  }

  Future<void> _carregar(int pagina) async {
    setState(() {
      _pagina = pagina;
      _resultado = context.read<AppEstado>().historico(pagina);
    });
    try {
      await _resultado;
    } catch (_) {
      // A tela de erro é exibida pelo FutureBuilder.
    }
  }

  @override
  Widget build(BuildContext context) => FutureBuilder(
    future: _resultado,
    builder: (context, snapshot) {
      if (snapshot.connectionState == ConnectionState.waiting) {
        return const Center(child: CircularProgressIndicator());
      }
      if (snapshot.hasError) {
        return Center(
          child: Mensagem(
            Icons.receipt_long_outlined,
            'Não foi possível carregar seus pedidos',
            texto: mensagemErro(snapshot.error!),
            acao: FilledButton(
              onPressed: () => _carregar(_pagina),
              child: const Text('Tentar novamente'),
            ),
          ),
        );
      }
      final pagina = snapshot.data!;
      return RefreshIndicator(
        onRefresh: () => _carregar(_pagina),
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16),
          children: [
            const Text('Acompanhe os pedidos feitos pelo aplicativo.'),
            const SizedBox(height: 12),
            if (pagina.pedidos.isEmpty)
              const Mensagem(
                Icons.receipt_long_outlined,
                'Você ainda não tem pedidos',
              ),
            for (final pedido in pagina.pedidos)
              Padding(
                padding: const EdgeInsets.only(bottom: 8),
                child: Card(
                  child: ListTile(
                    contentPadding: const EdgeInsets.symmetric(
                      horizontal: 16,
                      vertical: 6,
                    ),
                    title: Text(
                      pedido.status,
                      style: const TextStyle(fontWeight: FontWeight.bold),
                    ),
                    subtitle: Text(
                      '${dataHora(pedido.criadoEm)}\n${pedido.quantidadeItens} itens · ${moeda(pedido.total)}',
                    ),
                    trailing: const Icon(Icons.chevron_right),
                    onTap: () => Navigator.pushNamed(
                      context,
                      Rotas.pedido,
                      arguments: pedido.id,
                    ),
                  ),
                ),
              ),
            if (pagina.totalPaginas > 1)
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  IconButton(
                    tooltip: 'Página anterior',
                    onPressed: pagina.pagina > 1
                        ? () => _carregar(pagina.pagina - 1)
                        : null,
                    icon: const Icon(Icons.chevron_left),
                  ),
                  Text('${pagina.pagina} / ${pagina.totalPaginas}'),
                  IconButton(
                    tooltip: 'Próxima página',
                    onPressed: pagina.pagina < pagina.totalPaginas
                        ? () => _carregar(pagina.pagina + 1)
                        : null,
                    icon: const Icon(Icons.chevron_right),
                  ),
                ],
              ),
          ],
        ),
      );
    },
  );
}

class PedidoScreen extends StatefulWidget {
  final String id;
  final Pedido? confirmado;
  const PedidoScreen(this.id, {super.key, this.confirmado});

  @override
  State<PedidoScreen> createState() => _PedidoScreenState();
}

class _PedidoScreenState extends State<PedidoScreen> {
  late Future<Pedido> _resultado;
  String? _conta;
  bool _ocupado = false;

  @override
  void initState() {
    super.initState();
    _conta = context.read<AppEstado>().usuario?.id;
    _resultado = widget.confirmado == null
        ? context.read<AppEstado>().detalhe(widget.id)
        : Future.value(widget.confirmado);
  }

  Future<void> _carregar() async {
    setState(() => _resultado = context.read<AppEstado>().detalhe(widget.id));
    try {
      await _resultado;
    } catch (_) {
      // A tela de erro é exibida pelo FutureBuilder.
    }
  }

  Future<void> _repetir() async {
    final estado = context.read<AppEstado>();
    if (estado.carrinho.isNotEmpty) {
      final confirmar = await showDialog<bool>(
        context: context,
        builder: (context) => AlertDialog(
          title: const Text('Substituir o carrinho?'),
          content: const Text(
            'O carrinho receberá os produtos deste pedido que ainda estiverem disponíveis.',
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Cancelar'),
            ),
            FilledButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Substituir'),
            ),
          ],
        ),
      );
      if (confirmar != true || !mounted) return;
    }
    setState(() => _ocupado = true);
    try {
      final repeticao = await estado.repetir(widget.id);
      if (!mounted) return;
      if (repeticao.indisponiveis.isNotEmpty) {
        await showDialog<void>(
          context: context,
          builder: (context) => AlertDialog(
            title: const Text('Itens indisponíveis'),
            content: SingleChildScrollView(
              child: Text(repeticao.indisponiveis.join('\n')),
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('OK'),
              ),
            ],
          ),
        );
      }
      if (mounted && repeticao.itens.isNotEmpty) {
        await Navigator.pushReplacementNamed(context, Rotas.carrinho);
      }
    } catch (erro) {
      if (mounted) avisar(context, mensagemErro(erro));
    } finally {
      if (mounted) setState(() => _ocupado = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final usuario = context.watch<AppEstado>().usuario;
    final confirmado = widget.confirmado != null;
    return PopScope(
      canPop: !_ocupado,
      child: Scaffold(
        appBar: AppBar(
          title: Text(confirmado ? 'Pedido registrado' : 'Detalhes do pedido'),
        ),
        body: SafeArea(
          child: usuario == null || usuario.id != _conta
              ? const Center(
                  child: Mensagem(
                    Icons.lock_outline,
                    'Sua sessão terminou',
                    texto: 'Volte e entre novamente para abrir o pedido.',
                  ),
                )
              : FutureBuilder(
                  future: _resultado,
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Center(child: CircularProgressIndicator());
                    }
                    if (snapshot.hasError) {
                      return Center(
                        child: Mensagem(
                          Icons.receipt_long_outlined,
                          'Detalhes indisponíveis',
                          texto: mensagemErro(snapshot.error!),
                          acao: FilledButton(
                            onPressed: _carregar,
                            child: const Text('Tentar novamente'),
                          ),
                        ),
                      );
                    }
                    final pedido = snapshot.data!;
                    return RefreshIndicator(
                      onRefresh: _carregar,
                      child: ListView(
                        physics: const AlwaysScrollableScrollPhysics(),
                        padding: const EdgeInsets.all(16),
                        children: [
                          Aviso(
                            pedido.status,
                            icone: confirmado
                                ? Icons.check_circle_outline
                                : Icons.receipt_long,
                          ),
                          const SizedBox(height: 12),
                          SelectableText('Pedido ${pedido.id}'),
                          Text(dataHora(pedido.criadoEm)),
                          const SizedBox(height: 12),
                          Card(
                            child: Padding(
                              padding: const EdgeInsets.all(16),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    pedido.recebimento == 'Entrega'
                                        ? 'Entrega no endereço'
                                        : 'Retirada no balcão',
                                    style: Theme.of(context)
                                        .textTheme
                                        .titleMedium,
                                  ),
                                  if (pedido.endereco?.isNotEmpty == true)
                                    Text(pedido.endereco!),
                                  const SizedBox(height: 8),
                                  Text(
                                    '${pagamentos[pedido.pagamento] ?? pedido.pagamento} · ${pedido.pago ? 'Pago' : 'Aguardando pagamento'}',
                                  ),
                                  if (pedido.trocoPara != null)
                                    Text(
                                      'Troco para ${moeda(pedido.trocoPara!)}',
                                    ),
                                  if (pedido.observacoes?.isNotEmpty == true)
                                    Padding(
                                      padding: const EdgeInsets.only(top: 8),
                                      child: Text(pedido.observacoes!),
                                    ),
                                ],
                              ),
                            ),
                          ),
                          const SizedBox(height: 12),
                          ResumoValores(pedido),
                          const SizedBox(height: 14),
                          if (pedido.whatsAppUrl.isNotEmpty &&
                              pedido.status != 'Cancelado') ...[
                            FilledButton.icon(
                              onPressed: () async {
                                try {
                                  await abrirWhatsApp(pedido.whatsAppUrl);
                                } catch (erro) {
                                  if (context.mounted) {
                                    avisar(context, mensagemErro(erro));
                                  }
                                }
                              },
                              icon: const Icon(Icons.chat_outlined),
                              label: const Text('Abrir conversa no WhatsApp'),
                            ),
                            const SizedBox(height: 8),
                            const Text(
                              'Confira a mensagem e envie pela conversa quando quiser falar com a loja.',
                            ),
                          ],
                          if (!confirmado)
                            OutlinedButton(
                              onPressed: _ocupado ? null : _repetir,
                              child: const Text('Pedir novamente'),
                            ),
                          if (confirmado)
                            OutlinedButton(
                              onPressed: () => Navigator.popUntil(
                                context,
                                ModalRoute.withName(Rotas.home),
                              ),
                              child: const Text('Voltar ao cardápio'),
                            ),
                        ],
                      ),
                    );
                  },
                ),
        ),
      ),
    );
  }
}
