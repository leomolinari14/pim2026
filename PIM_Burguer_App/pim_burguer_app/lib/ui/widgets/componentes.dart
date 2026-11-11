import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../core/formato.dart';
import '../../core/rotas.dart';
import '../../data/api/api_cliente.dart';
import '../../models/pedido.dart';

String mensagemErro(Object erro) => switch (erro) {
  ApiErro() => erro.mensagem,
  PlatformException() =>
    erro.message ?? 'Não foi possível acessar os dados locais.',
  _ => 'Não foi possível concluir a operação. Tente novamente.',
};

void avisar(BuildContext context, String texto) {
  ScaffoldMessenger.of(context)
    ..hideCurrentSnackBar()
    ..showSnackBar(SnackBar(content: Text(texto)));
}

Future<void> abrirWhatsApp(String link) async {
  final uri = Uri.tryParse(link);
  if (uri == null ||
      uri.scheme != 'https' ||
      !const {
        'wa.me',
        'api.whatsapp.com',
        'www.whatsapp.com',
      }.contains(uri.host)) {
    throw const ApiErro('Não foi possível abrir o WhatsApp.');
  }
  await const MethodChannel('pim_burguer/navegacao')
      .invokeMethod<void>('abrirWhatsApp', {'url': uri.toString()});
}

class Mensagem extends StatelessWidget {
  final IconData icone;
  final String titulo;
  final String? texto;
  final Widget? acao;
  const Mensagem(this.icone, this.titulo, {super.key, this.texto, this.acao});

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 16),
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icone, size: 36, color: Theme.of(context).colorScheme.primary),
        const SizedBox(height: 10),
        Text(
          titulo,
          style: Theme.of(context).textTheme.titleLarge,
          textAlign: TextAlign.center,
        ),
        if (texto != null)
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Text(texto!, textAlign: TextAlign.center),
          ),
        if (acao != null)
          Padding(padding: const EdgeInsets.only(top: 14), child: acao!),
      ],
    ),
  );
}

class Aviso extends StatelessWidget {
  final String texto;
  final IconData icone;
  const Aviso(this.texto, {super.key, this.icone = Icons.info_outline});

  @override
  Widget build(BuildContext context) {
    final cores = Theme.of(context).colorScheme;
    return Container(
      decoration: BoxDecoration(
        color: cores.primaryContainer,
        borderRadius: BorderRadius.circular(12),
      ),
      padding: const EdgeInsets.all(12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icone, color: cores.onPrimaryContainer, size: 20),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              texto,
              style: TextStyle(color: cores.onPrimaryContainer),
            ),
          ),
        ],
      ),
    );
  }
}

class EntrarAviso extends StatelessWidget {
  const EntrarAviso({super.key});

  @override
  Widget build(BuildContext context) => Center(
    child: SingleChildScrollView(
      child: Mensagem(
        Icons.person_outline,
        'Entre na sua conta',
        texto: 'Use a mesma conta do site PIM Burguer.',
        acao: FilledButton(
          onPressed: () => Navigator.pushNamed(context, Rotas.login),
          child: const Text('Entrar ou criar conta'),
        ),
      ),
    ),
  );
}

class Campo extends StatelessWidget {
  final TextEditingController controller;
  final String rotulo;
  final int limite;
  final TextInputType? teclado;
  final bool senha;
  final bool obrigatorio;
  final int linhas;
  final String? Function(String)? validar;
  final ValueChanged<String>? aoMudar;

  const Campo(
    this.controller,
    this.rotulo,
    this.limite, {
    super.key,
    this.teclado,
    this.senha = false,
    this.obrigatorio = true,
    this.linhas = 1,
    this.validar,
    this.aoMudar,
  });

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 12),
    child: TextFormField(
      controller: controller,
      decoration: InputDecoration(labelText: rotulo, counterText: ''),
      keyboardType: senha ? TextInputType.visiblePassword : teclado,
      textInputAction: linhas > 1
          ? TextInputAction.newline
          : TextInputAction.next,
      maxLength: limite,
      maxLines: linhas,
      obscureText: senha,
      autocorrect: !senha,
      enableSuggestions: !senha,
      onChanged: aoMudar,
      validator: (valor) {
        final texto = senha ? (valor ?? '') : (valor ?? '').trim();
        if (obrigatorio && texto.isEmpty) return 'Preencha este campo.';
        return validar?.call(texto);
      },
    ),
  );
}

class ResumoValores extends StatelessWidget {
  final ValoresPedido valores;
  const ResumoValores(this.valores, {super.key});

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Resumo do pedido',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 8),
          for (final item in valores.itens)
            _linha('${item.quantidade} × ${item.nome}', moeda(item.subtotal)),
          const Divider(),
          _linha('Subtotal', moeda(valores.subtotal)),
          _linha('Entrega', moeda(valores.taxaEntrega)),
          if (valores.desconto > 0)
            _linha(
              'Cupom ${valores.codigoCupom ?? ''}',
              '−${moeda(valores.desconto)}',
            ),
          const Divider(),
          _linha('Total', moeda(valores.total), negrito: true),
        ],
      ),
    ),
  );

  Widget _linha(String titulo, String valor, {bool negrito = false}) {
    final estilo = negrito
        ? const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)
        : null;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Expanded(child: Text(titulo, style: estilo)),
          const SizedBox(width: 8),
          Text(valor, style: estilo),
        ],
      ),
    );
  }
}
