import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

class ApiErro implements Exception {
  final String mensagem;
  final int? status;
  const ApiErro(this.mensagem, [this.status]);
  @override
  String toString() => mensagem;
}

class ApiCliente {
  final Uri base;
  final http.Client _http;

  ApiCliente(this.base, {http.Client? httpClient})
    : _http = httpClient ?? http.Client();

  Future<Object?> enviar(
    String metodo,
    String caminho, {
    String? token,
    Map<String, Object?>? corpo,
    Map<String, String>? consulta,
  }) async {
    final uri = base.resolve(caminho).replace(queryParameters: consulta);
    final req = http.Request(metodo, uri)
      ..followRedirects = false
      ..headers['Accept'] = 'application/json';
    if (token != null) req.headers['Authorization'] = 'Bearer $token';
    if (corpo != null) {
      req.headers['Content-Type'] = 'application/json; charset=utf-8';
      req.body = jsonEncode(corpo);
    }

    final http.Response resposta;
    try {
      resposta = await _http
          .send(req)
          .then(http.Response.fromStream)
          .timeout(const Duration(seconds: 12));
    } on TimeoutException {
      throw const ApiErro('A API demorou a responder. Tente novamente.');
    } on http.ClientException {
      throw const ApiErro(
        'Sem conexão com a API. Verifique a rede e se o servidor está ligado.',
      );
    }

    final status = resposta.statusCode;
    final sucesso = status >= 200 && status < 300;
    Object? dados;
    if (resposta.bodyBytes.isNotEmpty) {
      try {
        dados = jsonDecode(utf8.decode(resposta.bodyBytes));
      } on FormatException {
        if (sucesso) {
          throw const ApiErro('A API devolveu uma resposta inesperada.');
        }
      }
    }
    if (sucesso) return dados;

    final problema = dados is Map<String, dynamic>
        ? dados
        : const <String, dynamic>{};
    final erros = problema['errors'];
    final validacao = erros is Map
        ? erros.values
              .whereType<List>()
              .expand((e) => e.whereType<String>())
              .join('\n')
        : '';
    final detalhe = problema['detail'] as String?;
    final mensagem = switch (status) {
      401 => detalhe ?? 'Entre novamente para continuar.',
      403 => 'Esta conta não tem acesso a esse recurso.',
      404 => 'Registro não encontrado.',
      429 => 'Muitas tentativas. Aguarde um pouco e tente novamente.',
      >= 500 => 'O serviço está indisponível no momento. Tente novamente.',
      _ =>
        validacao.isNotEmpty
            ? validacao
            : detalhe ?? 'Revise os dados e tente novamente.',
    };
    throw ApiErro(mensagem, status);
  }

  void fechar() => _http.close();
}
