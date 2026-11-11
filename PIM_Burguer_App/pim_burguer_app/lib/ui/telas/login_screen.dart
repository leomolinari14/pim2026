import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/app_estado.dart';
import '../../core/formato.dart';
import '../../core/tema.dart';
import '../widgets/componentes.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

String? validarNome(String texto) =>
    texto.length < 2 ? 'Informe o nome completo.' : null;
String? validarTelefone(String texto) {
  final telefone = digitos(texto);
  return telefone.length < 10 || telefone.length > 11
      ? 'Informe um telefone com DDD.'
      : null;
}

String? validarCpf(String texto) =>
    cpfValido(texto) ? null : 'Informe um CPF válido.';

String? validarSenha(String texto) =>
    RegExp(r'^(?=.*[A-Za-z])(?=.*[0-9]).{8,100}$').hasMatch(texto)
    ? null
    : 'Use 8 caracteres ou mais, com letras e números.';

class _LoginScreenState extends State<LoginScreen> {
  final _form = GlobalKey<FormState>();
  final _email = TextEditingController();
  final _senha = TextEditingController();
  final _nome = TextEditingController();
  final _telefone = TextEditingController();
  final _cpf = TextEditingController();
  final _confirmar = TextEditingController();
  bool _cadastro = false;
  bool _ocupado = false;
  String? _erro;

  Future<void> _enviar() async {
    if (_ocupado || !_form.currentState!.validate()) return;
    FocusScope.of(context).unfocus();
    final estado = context.read<AppEstado>();
    setState(() {
      _ocupado = true;
      _erro = null;
    });
    try {
      await estado.autenticar(_cadastro, {
        'email': _email.text.trim(),
        'senha': _senha.text,
        if (_cadastro) ...{
          'nome': _nome.text.trim(),
          'telefone': digitos(_telefone.text),
          'cpf': digitos(_cpf.text),
          'confirmarSenha': _confirmar.text,
        },
      });
      if (mounted) Navigator.pop(context, true);
    } catch (erro) {
      if (mounted) setState(() => _erro = mensagemErro(erro));
    } finally {
      if (mounted) setState(() => _ocupado = false);
    }
  }

  @override
  void dispose() {
    for (final controller in [
      _email,
      _senha,
      _nome,
      _telefone,
      _cpf,
      _confirmar,
    ]) {
      controller.dispose();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => PopScope(
    canPop: !_ocupado,
    child: Scaffold(
      appBar: AppBar(title: Text(_cadastro ? 'Criar conta' : 'Entrar')),
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            const Align(
              alignment: Alignment.centerLeft,
              child: Icon(Icons.lunch_dining, size: 42, color: marca),
            ),
            const SizedBox(height: 10),
            Text(
              _cadastro ? 'Sua conta PIM Burguer' : 'Bom ter você aqui',
              style: Theme.of(context).textTheme.headlineSmall
                  ?.copyWith(fontWeight: FontWeight.w800),
            ),
            const SizedBox(height: 6),
            Text(
              _cadastro
                  ? 'Cadastre-se para fazer pedidos e acompanhar a entrega.'
                  : 'Use seu e-mail e a senha da sua conta PIM Burguer.',
            ),
            const SizedBox(height: 18),
            Form(
              key: _form,
              child: Column(
                children: [
                  if (_cadastro) ...[
                    Campo(_nome, 'Nome completo', 100, validar: validarNome),
                    Campo(
                      _cpf,
                      'CPF',
                      14,
                      teclado: TextInputType.number,
                      validar: validarCpf,
                    ),
                    Campo(
                      _telefone,
                      'Telefone com DDD',
                      20,
                      teclado: TextInputType.phone,
                      validar: validarTelefone,
                    ),
                  ],
                  Campo(
                    _email,
                    'E-mail',
                    180,
                    teclado: TextInputType.emailAddress,
                    validar: (s) => RegExp(r'^[^@\s]+@[^@\s]+$').hasMatch(s)
                        ? null
                        : 'Informe um e-mail válido.',
                  ),
                  Campo(
                    _senha,
                    'Senha',
                    100,
                    senha: true,
                    validar: _cadastro ? validarSenha : null,
                  ),
                  if (_cadastro)
                    Campo(
                      _confirmar,
                      'Confirmar senha',
                      100,
                      senha: true,
                      validar: (s) => s == _senha.text
                          ? null
                          : 'As senhas precisam ser iguais.',
                    ),
                ],
              ),
            ),
            if (_erro != null) ...[Aviso(_erro!), const SizedBox(height: 12)],
            FilledButton(
              onPressed: _ocupado ? null : _enviar,
              child: Text(
                _ocupado
                    ? 'Aguarde…'
                    : _cadastro
                    ? 'Criar minha conta'
                    : 'Entrar',
              ),
            ),
            TextButton(
              onPressed: _ocupado
                  ? null
                  : () => setState(() {
                      _cadastro = !_cadastro;
                      _erro = null;
                      _senha.clear();
                      _confirmar.clear();
                    }),
              child: Text(_cadastro ? 'Já tenho uma conta' : 'Criar uma conta'),
            ),
          ],
        ),
      ),
    ),
  );
}
