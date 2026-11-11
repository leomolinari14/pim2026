import 'package:flutter/foundation.dart';

Uri enderecoApi() {
  const padrao = kDebugMode ? 'http://10.0.2.2:5181/mobile' : '';
  const valor = String.fromEnvironment('API_BASE_URL', defaultValue: padrao);
  final uri = Uri.tryParse(valor.trim());
  if (uri == null ||
      !uri.hasAuthority ||
      uri.host.isEmpty ||
      uri.userInfo.isNotEmpty ||
      uri.hasQuery ||
      uri.hasFragment ||
      (uri.scheme != 'https' && !(kDebugMode && uri.scheme == 'http'))) {
    throw const FormatException(
      'Configure API_BASE_URL com o endereço da API.',
    );
  }
  final caminho = uri.path.replaceFirst(RegExp(r'/+$'), '');
  final prefixo = caminho.isEmpty ? '/mobile' : caminho;
  return uri.replace(path: '$prefixo/');
}
