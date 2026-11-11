import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';

import '../../core/app_estado.dart';
import '../../core/formato.dart';
import '../../core/rotas.dart';
import '../../models/pedido.dart';
import '../widgets/componentes.dart';
import 'login_screen.dart';

class ContaScreen extends StatelessWidget {
  const ContaScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final estado = context.watch<AppEstado>();
    final usuario = estado.usuario;
    if (usuario == null) return const EntrarAviso();
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Card(
          child: ListTile(
            leading: const CircleAvatar(child: Icon(Icons.person_outline)),
            title: Text(
              usuario.nome,
              style: const TextStyle(fontWeight: FontWeight.w800),
            ),
            subtitle: Text(usuario.email),
          ),
        ),
        const SizedBox(height: 12),
        Card(
          child: Column(
            children: [
              _opcao(
                context,
                Icons.person_outline,
                'Editar meus dados',
                Rotas.perfil,
              ),
              _opcao(
                context,
                Icons.local_offer_outlined,
                'Meus cupons',
                Rotas.cupons,
              ),
              _opcao(context, Icons.lock_outline, 'Alterar senha', Rotas.senha),
            ],
          ),
        ),
        const SizedBox(height: 12),
        OutlinedButton.icon(
          onPressed: () async {
            try {
              await estado.sair();
            } catch (erro) {
              if (context.mounted) avisar(context, mensagemErro(erro));
            }
          },
          icon: const Icon(Icons.logout),
          label: const Text('Sair da conta'),
        ),
      ],
    );
  }

  Widget _opcao(
    BuildContext context,
    IconData icone,
    String titulo,
    String rota,
  ) => ListTile(
    leading: Icon(icone),
    title: Text(titulo),
    trailing: const Icon(Icons.chevron_right),
    onTap: () => Navigator.pushNamed(context, rota),
  );
}

class PerfilScreen extends StatefulWidget {
  final bool senha;
  const PerfilScreen({super.key, this.senha = false});

  @override
  State<PerfilScreen> createState() => _PerfilScreenState();
}

class _PerfilScreenState extends State<PerfilScreen> {
  final _form = GlobalKey<FormState>();
  final _nome = TextEditingController();
  final _telefone = TextEditingController();
  final _senhaAtual = TextEditingController();
  final _novaSenha = TextEditingController();
  final _confirmarSenha = TextEditingController();
  String? _conta;
  String? _erro;
  bool _ocupado = false;

  @override
  void initState() {
    super.initState();
    final usuario = context.read<AppEstado>().usuario;
    _conta = usuario?.id;
    _nome.text = usuario?.nome ?? '';
    _telefone.text = usuario?.telefone ?? '';
  }

  @override
  void dispose() {
    for (final controller in [
      _nome,
      _telefone,
      _senhaAtual,
      _novaSenha,
      _confirmarSenha,
    ]) {
      controller.dispose();
    }
    super.dispose();
  }

  Future<void> _salvar() async {
    if (_ocupado || !_form.currentState!.validate()) return;
    FocusScope.of(context).unfocus();
    final estado = context.read<AppEstado>();
    setState(() {
      _erro = null;
      _ocupado = true;
    });
    try {
      if (widget.senha) {
        await estado.alterarSenha({
          'senhaAtual': _senhaAtual.text,
          'novaSenha': _novaSenha.text,
          'confirmarSenha': _confirmarSenha.text,
        });
      } else {
        await estado.atualizarPerfil(
          _nome.text.trim(),
          digitos(_telefone.text),
        );
      }
      if (mounted) {
        Navigator.pop(context);
        avisar(
          context,
          widget.senha
              ? 'Senha alterada. Entre novamente.'
              : 'Dados atualizados.',
        );
      }
    } catch (erro) {
      if (mounted) setState(() => _erro = mensagemErro(erro));
    } finally {
      if (mounted) setState(() => _ocupado = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final usuario = context.watch<AppEstado>().usuario;
    return PopScope(
      canPop: !_ocupado,
      child: Scaffold(
        appBar: AppBar(
          title: Text(widget.senha ? 'Alterar senha' : 'Meus dados'),
        ),
        body: SafeArea(
          child: usuario == null || usuario.id != _conta
              ? const _SessaoTerminou()
              : Form(
                  key: _form,
                  child: ListView(
                    padding: const EdgeInsets.all(16),
                    children: [
                      if (widget.senha) ...[
                        const Aviso(
                          'Ao alterar a senha, entre novamente nos seus dispositivos.',
                        ),
                        const SizedBox(height: 12),
                        Campo(_senhaAtual, 'Senha atual', 100, senha: true),
                        Campo(
                          _novaSenha,
                          'Nova senha',
                          100,
                          senha: true,
                          validar: validarSenha,
                        ),
                        Campo(
                          _confirmarSenha,
                          'Confirmar nova senha',
                          100,
                          senha: true,
                          validar: (s) => s == _novaSenha.text
                              ? null
                              : 'As senhas precisam ser iguais.',
                        ),
                      ] else ...[
                        Card(
                          child: ListTile(
                            leading: const Icon(Icons.email_outlined),
                            title: Text(usuario.email),
                          ),
                        ),
                        const SizedBox(height: 12),
                        Campo(
                          _nome,
                          'Nome completo',
                          100,
                          validar: validarNome,
                        ),
                        Campo(
                          _telefone,
                          'Telefone com DDD',
                          20,
                          teclado: TextInputType.phone,
                          validar: validarTelefone,
                        ),
                      ],
                      if (_erro != null) ...[
                        Aviso(_erro!),
                        const SizedBox(height: 12),
                      ],
                      FilledButton(
                        onPressed: _ocupado ? null : _salvar,
                        child: Text(
                          _ocupado ? 'Salvando…' : 'Salvar alterações',
                        ),
                      ),
                    ],
                  ),
                ),
        ),
      ),
    );
  }
}

class _SessaoTerminou extends StatelessWidget {
  const _SessaoTerminou();

  @override
  Widget build(BuildContext context) => const Center(
    child: Mensagem(
      Icons.lock_outline,
      'Sua sessão terminou',
      texto: 'Volte e entre novamente para continuar.',
    ),
  );
}

class CuponsScreen extends StatefulWidget {
  final bool selecionar;
  const CuponsScreen({super.key, this.selecionar = false});

  @override
  State<CuponsScreen> createState() => _CuponsScreenState();
}

class _CuponsScreenState extends State<CuponsScreen> {
  late Future<List<Cupom>> _cupons;
  String? _conta;
  final _codigo = TextEditingController();
  final _form = GlobalKey<FormState>();
  bool _adicionando = false;
  String? _erroAdicionar;

  @override
  void initState() {
    super.initState();
    final estado = context.read<AppEstado>();
    _conta = estado.usuario?.id;
    _cupons = estado.usuario == null ? Future.value([]) : estado.cupons();
  }

  @override
  void dispose() {
    _codigo.dispose();
    super.dispose();
  }

  Future<void> _adicionar() async {
    if (_adicionando || !_form.currentState!.validate()) return;
    final estado = context.read<AppEstado>();
    FocusScope.of(context).unfocus();
    setState(() {
      _adicionando = true;
      _erroAdicionar = null;
    });
    try {
      await estado.adicionarCupom(_codigo.text.trim().toUpperCase());
      if (!mounted || estado.usuario?.id != _conta) return;
      setState(() {
        _codigo.clear();
        _cupons = estado.cupons();
      });
      avisar(context, 'Cupom adicionado.');
    } catch (erro) {
      if (mounted) setState(() => _erroAdicionar = mensagemErro(erro));
    } finally {
      if (mounted) setState(() => _adicionando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final estado = context.watch<AppEstado>();
    return Scaffold(
      appBar: AppBar(
        title: const Text('Meus cupons'),
        actions: [
          if (estado.usuario != null && estado.usuario?.id == _conta)
            IconButton(
              tooltip: 'Atualizar cupons',
              icon: const Icon(Icons.refresh),
              onPressed: () => setState(() => _cupons = estado.cupons()),
            ),
        ],
      ),
      body: SafeArea(
        child: estado.usuario == null || estado.usuario?.id != _conta
            ? const _SessaoTerminou()
            : ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Form(
                        key: _form,
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            Text(
                              'Adicionar cupom',
                              style: Theme.of(context).textTheme.titleLarge,
                            ),
                            const SizedBox(height: 8),
                            const Text('Digite o código recebido da loja.'),
                            const SizedBox(height: 12),
                            TextFormField(
                              controller: _codigo,
                              enabled: !_adicionando,
                              maxLength: 24,
                              textCapitalization: TextCapitalization.characters,
                              textInputAction: TextInputAction.done,
                              decoration: const InputDecoration(
                                labelText: 'Código do cupom',
                                prefixIcon: Icon(Icons.local_offer_outlined),
                              ),
                              validator: (valor) =>
                                  RegExp(r'^[A-Za-z0-9_-]{3,24}$')
                                      .hasMatch(valor?.trim() ?? '')
                                  ? null
                                  : 'Informe um código com 3 a 24 caracteres.',
                              onFieldSubmitted: (_) => _adicionar(),
                            ),
                            if (_erroAdicionar != null) Aviso(_erroAdicionar!),
                            FilledButton.icon(
                              onPressed: _adicionando ? null : _adicionar,
                              icon: const Icon(Icons.add),
                              label: Text(
                                _adicionando
                                    ? 'Adicionando…'
                                    : 'Adicionar cupom',
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  FutureBuilder<List<Cupom>>(
                    future: _cupons,
                    builder: (context, snapshot) {
                      if (snapshot.connectionState == ConnectionState.waiting) {
                        return const Center(child: CircularProgressIndicator());
                      }
                      if (snapshot.hasError) {
                        return Center(
                          child: Mensagem(
                            Icons.local_offer_outlined,
                            'Cupons indisponíveis',
                            texto: mensagemErro(snapshot.error!),
                            acao: FilledButton(
                              onPressed: () =>
                                  setState(() => _cupons = estado.cupons()),
                              child: const Text('Tentar novamente'),
                            ),
                          ),
                        );
                      }
                      final cupons = snapshot.data!;
                      return Column(
                        children: [
                          if (cupons.isEmpty)
                            const Mensagem(
                              Icons.local_offer_outlined,
                              'Nenhum cupom disponível',
                            ),
                          for (final cupom in cupons)
                            Padding(
                              padding: const EdgeInsets.only(bottom: 10),
                              child: Card(
                                child: Padding(
                                  padding: const EdgeInsets.all(16),
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        cupom.codigo,
                                        style: Theme.of(context)
                                            .textTheme
                                            .titleLarge,
                                      ),
                                      Text(
                                        cupom.percentual
                                            ? '${cupom.valor}% de desconto'
                                            : '${moeda(cupom.valor)} de desconto',
                                      ),
                                      Text(
                                        'Compra mínima de ${moeda(cupom.compraMinima)}',
                                      ),
                                      Text(
                                        'Válido até ${cupom.validoAte.split('-').reversed.join('/')}',
                                      ),
                                      const SizedBox(height: 8),
                                      OutlinedButton(
                                        onPressed:
                                            cupom.utilizado || cupom.esgotado
                                            ? null
                                            : () async {
                                                if (widget.selecionar) {
                                                  Navigator.pop(
                                                    context,
                                                    cupom.codigo,
                                                  );
                                                } else {
                                                  await Clipboard.setData(
                                                    ClipboardData(
                                                      text: cupom.codigo,
                                                    ),
                                                  );
                                                  if (context.mounted) {
                                                    avisar(
                                                      context,
                                                      'Código copiado.',
                                                    );
                                                  }
                                                }
                                              },
                                        child: Text(
                                          cupom.esgotado
                                              ? 'Esgotado'
                                              : cupom.utilizado
                                              ? 'Já utilizado'
                                              : widget.selecionar
                                              ? 'Usar no checkout'
                                              : 'Copiar código',
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ),
                            ),
                        ],
                      );
                    },
                  ),
                ],
              ),
      ),
    );
  }
}
