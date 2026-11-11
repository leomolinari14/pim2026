import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/app_estado.dart';
import '../../core/formato.dart';
import '../../core/rotas.dart';
import '../../models/pedido.dart';
import '../widgets/componentes.dart';
import 'login_screen.dart';

class CheckoutScreen extends StatefulWidget {
  const CheckoutScreen({super.key});

  @override
  State<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends State<CheckoutScreen> {
  final _form = GlobalKey<FormState>();
  final _campos = {
    for (final nome in [
      'nomeCliente',
      'cpfCliente',
      'telefone',
      'cep',
      'logradouro',
      'numero',
      'complemento',
      'bairro',
      'cidade',
      'uf',
      'codigoCupom',
      'trocoPara',
      'observacoes',
    ])
      nome: TextEditingController(),
  };
  String _recebimento = 'Retirada';
  String _pagamento = 'Pix';
  Orcamento? _orcamento;
  late Future<bool> _pendencia;
  bool _ocupado = false;
  String? _erro;

  bool get _entrega => _recebimento == 'Entrega';
  bool get _dinheiro => _pagamento == 'Dinheiro';
  String _valor(String chave) => _campos[chave]!.text.trim();

  @override
  void initState() {
    super.initState();
    final estado = context.read<AppEstado>();
    _pendencia = estado.temPendencia();
    final usuario = estado.usuario;
    _campos['nomeCliente']!.text = usuario?.nome ?? '';
    _campos['telefone']!.text = usuario?.telefone ?? '';
    _campos['cpfCliente']!.text = usuario?.cpf ?? '';
  }

  @override
  void dispose() {
    for (final campo in _campos.values) {
      campo.dispose();
    }
    super.dispose();
  }

  Map<String, Object?> _pedidoBase(AppEstado estado) => {
    'itens': itensSolicitados(estado.carrinho),
    'recebimento': _recebimento,
    'codigoCupom': _valor('codigoCupom').isEmpty
        ? null
        : _valor('codigoCupom').toUpperCase(),
  };

  num? _troco() => num.tryParse(_valor('trocoPara').replaceAll(',', '.'));

  Map<String, Object?> _pedidoCompleto(AppEstado estado) => {
    ..._pedidoBase(estado),
    'orcamentoHash': _orcamento!.hash,
    'nomeCliente': _valor('nomeCliente'),
    'cpfCliente': digitos(_valor('cpfCliente')),
    'telefone': digitos(_valor('telefone')),
    'pagamento': _pagamento,
    'trocoPara': _dinheiro ? _troco() : null,
    'observacoes': _valor('observacoes'),
    if (_entrega) ...{
      'cep': digitos(_valor('cep')),
      for (final chave in [
        'logradouro',
        'numero',
        'complemento',
        'bairro',
        'cidade',
      ])
        chave: _valor(chave),
      'uf': _valor('uf').toUpperCase(),
    },
  };

  Future<void> _acao({required bool enviar, bool recuperar = false}) async {
    if (_ocupado || (!recuperar && !_form.currentState!.validate())) return;
    FocusScope.of(context).unfocus();
    final estado = context.read<AppEstado>();
    setState(() {
      _ocupado = true;
      _erro = null;
    });
    try {
      if (!enviar) {
        final orcamento = await estado.orcar(_pedidoBase(estado));
        if (mounted) setState(() => _orcamento = orcamento);
        return;
      }
      final pedido = await estado.confirmar(
        recuperar ? null : _pedidoCompleto(estado),
      );
      if (mounted) {
        Navigator.pushReplacementNamed(
          context,
          Rotas.pedido,
          arguments: pedido,
        );
      }
    } catch (erro) {
      if (mounted) {
        setState(() {
          _erro = mensagemErro(erro);
          if (enviar && estado.usuario != null) {
            _pendencia = estado.temPendencia();
          }
          _orcamento = null;
        });
      }
    } finally {
      if (mounted) setState(() => _ocupado = false);
    }
  }

  void _invalidar() {
    if (_orcamento != null) setState(() => _orcamento = null);
  }

  @override
  Widget build(BuildContext context) {
    final estado = context.watch<AppEstado>();
    return PopScope(
      canPop: !_ocupado,
      child: Scaffold(
        appBar: AppBar(title: const Text('Finalizar pedido')),
        body: SafeArea(
          child: estado.usuario == null
              ? Center(
                  child: Mensagem(
                    Icons.lock_outline,
                    'Sua sessão terminou',
                    texto:
                        'Volte ao carrinho e entre novamente para continuar.',
                    acao: FilledButton(
                      onPressed: () => Navigator.pop(context),
                      child: const Text('Voltar ao carrinho'),
                    ),
                  ),
                )
              : FutureBuilder(
                  future: _pendencia,
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Center(child: CircularProgressIndicator());
                    }
                    if (snapshot.hasError) {
                      return Center(
                        child: Mensagem(
                          Icons.storage,
                          'Envio pendente indisponível',
                          texto: mensagemErro(snapshot.error!),
                          acao: FilledButton(
                            onPressed: () => setState(
                              () => _pendencia = estado.temPendencia(),
                            ),
                            child: const Text('Tentar novamente'),
                          ),
                        ),
                      );
                    }
                    if (snapshot.data == true) return _recuperacao();
                    if (estado.carrinho.isEmpty || estado.indisponiveis) {
                      return Center(
                        child: Mensagem(
                          Icons.shopping_bag_outlined,
                          estado.indisponiveis
                              ? 'Revise seu carrinho'
                              : 'Nenhum envio pendente',
                          texto: estado.indisponiveis
                              ? 'Retire os produtos indisponíveis e tente novamente.'
                              : null,
                          acao: FilledButton(
                            onPressed: () => Navigator.pop(context),
                            child: const Text('Voltar ao carrinho'),
                          ),
                        ),
                      );
                    }
                    return _checkout();
                  },
                ),
        ),
      ),
    );
  }

  Widget _recuperacao() => ListView(
    padding: const EdgeInsets.all(16),
    children: [
      const Aviso(
        'Há um envio sem confirmação neste aparelho. Verifique a tentativa anterior para evitar pedido duplicado.',
      ),
      if (_erro != null)
        Padding(padding: const EdgeInsets.only(top: 12), child: Aviso(_erro!)),
      const SizedBox(height: 14),
      FilledButton(
        onPressed: _ocupado ? null : () => _acao(enviar: true, recuperar: true),
        child: Text(_ocupado ? 'Verificando…' : 'Verificar pedido pendente'),
      ),
    ],
  );

  Widget _checkout() {
    final valores = _orcamento;
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        const Aviso(
          'O total é calculado pela loja. Confira o resumo antes de registrar o pedido.',
        ),
        const SizedBox(height: 16),
        Form(
          key: _form,
          child: AbsorbPointer(
            absorbing: _ocupado,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                _titulo('Contato'),
                _campo(
                  'nomeCliente',
                  'Nome completo',
                  100,
                  validar: validarNome,
                ),
                _campo(
                  'cpfCliente',
                  'CPF',
                  14,
                  teclado: TextInputType.number,
                  validar: validarCpf,
                ),
                _campo(
                  'telefone',
                  'Telefone com DDD',
                  20,
                  teclado: TextInputType.phone,
                  validar: validarTelefone,
                ),
                _titulo('Recebimento'),
                DropdownButtonFormField(
                  initialValue: _recebimento,
                  decoration: const InputDecoration(
                    labelText: 'Como você quer receber?',
                  ),
                  items: const [
                    DropdownMenuItem(
                      value: 'Retirada',
                      child: Text('Retirada no balcão'),
                    ),
                    DropdownMenuItem(
                      value: 'Entrega',
                      child: Text('Entrega no endereço'),
                    ),
                  ],
                  onChanged: (valor) => setState(() {
                    _recebimento = valor!;
                    _orcamento = null;
                  }),
                ),
                const SizedBox(height: 12),
                if (_entrega) ...[
                  _campo(
                    'cep',
                    'CEP',
                    9,
                    teclado: TextInputType.number,
                    validar: (s) => digitos(s).length == 8
                        ? null
                        : 'Informe um CEP de 8 dígitos.',
                  ),
                  _campo('logradouro', 'Rua ou avenida', 160),
                  _campo('numero', 'Número', 12),
                  _campo('complemento', 'Complemento', 80, obrigatorio: false),
                  _campo('bairro', 'Bairro', 80),
                  _campo('cidade', 'Cidade', 80),
                  _campo(
                    'uf',
                    'UF',
                    2,
                    validar: (s) =>
                        const {
                          'AC',
                          'AL',
                          'AP',
                          'AM',
                          'BA',
                          'CE',
                          'DF',
                          'ES',
                          'GO',
                          'MA',
                          'MT',
                          'MS',
                          'MG',
                          'PA',
                          'PB',
                          'PR',
                          'PE',
                          'PI',
                          'RJ',
                          'RN',
                          'RS',
                          'RO',
                          'RR',
                          'SC',
                          'SP',
                          'SE',
                          'TO',
                        }.contains(s.toUpperCase())
                        ? null
                        : 'Informe uma UF válida.',
                  ),
                ],
                _titulo('Pagamento e cupom'),
                DropdownButtonFormField(
                  initialValue: _pagamento,
                  decoration: const InputDecoration(
                    labelText: 'Forma de pagamento',
                  ),
                  items: [
                    for (final item in pagamentos.entries)
                      DropdownMenuItem(
                        value: item.key,
                        child: Text(item.value),
                      ),
                  ],
                  onChanged: (valor) => setState(() => _pagamento = valor!),
                ),
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 10),
                  child: Text('A loja combina a cobrança pelo atendimento.'),
                ),
                if (_dinheiro)
                  _campo(
                    'trocoPara',
                    'Troco para (opcional)',
                    9,
                    obrigatorio: false,
                    teclado: const TextInputType.numberWithOptions(
                      decimal: true,
                    ),
                    validar: (s) {
                      if (s.isEmpty) return null;
                      final valor = _troco();
                      if (!RegExp(r'^[0-9]+([,.][0-9]{1,2})?$').hasMatch(s) ||
                          valor == null ||
                          valor <= 0 ||
                          valor > 100000) {
                        return 'Informe um valor de troco válido.';
                      }
                      if (valores != null && valor < valores.total) {
                        return 'O troco deve cobrir o total.';
                      }
                      return null;
                    },
                  ),
                _campo(
                  'codigoCupom',
                  'Código do cupom (opcional)',
                  24,
                  obrigatorio: false,
                  aoMudar: (_) => _invalidar(),
                ),
                Align(
                  alignment: Alignment.centerLeft,
                  child: TextButton(
                    onPressed: () async {
                      final codigo = await Navigator.pushNamed<String>(
                        context,
                        Rotas.cupons,
                        arguments: true,
                      );
                      if (mounted && codigo != null) {
                        _campos['codigoCupom']!.text = codigo;
                        _invalidar();
                      }
                    },
                    child: const Text('Ver meus cupons'),
                  ),
                ),
                _campo(
                  'observacoes',
                  'Observações (opcional)',
                  500,
                  obrigatorio: false,
                  linhas: 3,
                ),
              ],
            ),
          ),
        ),
        if (valores != null) ...[
          ResumoValores(valores),
          const SizedBox(height: 12),
        ],
        if (_erro != null) ...[Aviso(_erro!), const SizedBox(height: 12)],
        if (_ocupado) const LinearProgressIndicator(),
        FilledButton(
          onPressed: _ocupado ? null : () => _acao(enviar: valores != null),
          child: Text(valores == null ? 'Revisar valores' : 'Confirmar pedido'),
        ),
      ],
    );
  }

  Widget _titulo(String texto) => Padding(
    padding: const EdgeInsets.only(bottom: 8),
    child: Text(texto, style: Theme.of(context).textTheme.titleLarge),
  );

  Widget _campo(
    String chave,
    String rotulo,
    int limite, {
    TextInputType? teclado,
    bool obrigatorio = true,
    int linhas = 1,
    String? Function(String)? validar,
    ValueChanged<String>? aoMudar,
  }) => Campo(
    _campos[chave]!,
    rotulo,
    limite,
    teclado: teclado,
    obrigatorio: obrigatorio,
    linhas: linhas,
    validar: validar,
    aoMudar: aoMudar,
  );
}
