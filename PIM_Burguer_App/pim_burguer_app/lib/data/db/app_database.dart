import 'dart:convert';

import 'package:flutter/services.dart';

import '../../models/pedido.dart';
import '../../models/produto.dart';

class AppDatabase {
  static const _canal = MethodChannel('pim_burguer/armazenamento');
  final String origem;

  const AppDatabase(this.origem);

  Future<T?> _executar<T>(
    String acao, [
    Map<String, Object?> dados = const {},
  ]) => _canal.invokeMethod<T>(acao, {'origem': origem, ...dados});

  Future<List<Produto>> lerProdutos() async =>
      (await _executar<List<dynamic>>('lerProdutos'))!
          .cast<Map<dynamic, dynamic>>()
          .map(Produto.fromLocal)
          .toList();

  Future<void> salvarProdutos(List<Produto> produtos) => _executar<void>(
    'salvarProdutos',
    {'produtos': produtos.map((p) => p.toLocal()).toList()},
  );

  Future<Map<int, int>> lerCarrinho() async => Map<int, int>.from(
    (await _executar<Map<dynamic, dynamic>>('lerCarrinho'))!,
  );

  Future<void> salvarCarrinho(Map<int, int> itens) =>
      _executar<void>('salvarCarrinho', {'itens': itens});

  Future<Map<String, dynamic>?> lerPendencia(String usuario) async {
    final json = await _executar<String>('lerPendencia', {'usuario': usuario});
    return json == null ? null : objeto(jsonDecode(json));
  }

  Future<void> salvarPendencia(String usuario, Map<String, Object?> dados) =>
      _executar<void>('salvarPendencia', {
        'usuario': usuario,
        'json': jsonEncode(dados),
      });

  Future<void> excluirPendencia(String usuario) =>
      _executar<void>('excluirPendencia', {'usuario': usuario});
}
