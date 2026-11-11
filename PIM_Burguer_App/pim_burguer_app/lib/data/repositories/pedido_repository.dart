import 'dart:convert';
import 'dart:math';

import 'package:uuid/uuid.dart';

import '../api/pedido_api.dart';
import '../db/app_database.dart';

class PedidoRepository {
  final PedidoApi api;
  final AppDatabase banco;

  const PedidoRepository(this.api, this.banco);

  Future<Map<String, dynamic>?> pendencia(String usuario) =>
      banco.lerPendencia(usuario);

  Future<Map<String, Object?>> preparar(
    String usuario,
    Map<String, Object?> dados,
  ) async {
    final aleatorio = Random.secure();
    final payload = {
      ...dados,
      'id': const Uuid().v4(),
      'tokenConsulta': base64UrlEncode(
        List.generate(32, (_) => aleatorio.nextInt(256)),
      ),
    };
    await banco.salvarPendencia(usuario, payload);
    return payload;
  }

  Future<void> concluir(String usuario) => banco.excluirPendencia(usuario);
}
