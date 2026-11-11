import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:provider/provider.dart';
import 'package:pim_burguer_app/core/app_estado.dart';
import 'package:pim_burguer_app/data/api/api_cliente.dart';
import 'package:pim_burguer_app/data/api/conta_api.dart';
import 'package:pim_burguer_app/data/api/pedido_api.dart';
import 'package:pim_burguer_app/data/api/produto_api.dart';
import 'package:pim_burguer_app/data/db/app_database.dart';
import 'package:pim_burguer_app/data/repositories/pedido_repository.dart';
import 'package:pim_burguer_app/data/repositories/produto_repository.dart';
import 'package:pim_burguer_app/ui/telas/perfil_screen.dart';

Map<String, Object?> cupom(
  String codigo, {
  bool umaVez = true,
  bool usado = false,
  int? limite,
  int usos = 0,
}) => {
  'codigo': codigo,
  'tipo': 'Percentual',
  'valor': 10,
  'compraMinima': 0,
  'validoAte': '2027-12-31',
  'utilizadoPeloCliente': usado,
  'umaVezPorCliente': umaVez,
  'limiteUsos': limite,
  'usos': usos,
};

http.Response json(Object body, [int status = 200]) => http.Response(
  jsonEncode(body),
  status,
  headers: {'content-type': 'application/json; charset=utf-8'},
);

Future<AppEstado> estadoPara(http.Client client) async {
  final api = ApiCliente(
    Uri.parse('http://localhost/mobile/'),
    httpClient: client,
  );
  const banco = AppDatabase('teste-cupons');
  final estado = AppEstado(
    api,
    ContaApi(api),
    ProdutoRepository(ProdutoApi(api), banco),
    PedidoRepository(PedidoApi(api), banco),
    banco,
  );
  await estado.autenticar(false, {
    'email': 'teste@example.invalid',
    'senha': 'Teste2026',
  });
  return estado;
}

http.Response login() => json({
  'token': 'token-teste',
  'expiraEm': DateTime.now().add(const Duration(minutes: 30)).toIso8601String(),
  'usuario': {
    'id': 'cliente-teste',
    'nome': 'Cliente',
    'email': 'teste@example.invalid',
    'telefone': '11999990000',
    'cpf': '11144477735',
    'perfil': 'Cliente',
  },
});

Future<void> abrir(WidgetTester tester, AppEstado estado) async {
  await tester.pumpWidget(
    ChangeNotifierProvider<AppEstado>(
      create: (_) => estado,
      child: const MaterialApp(home: CuponsScreen()),
    ),
  );
  await tester.pumpAndSettle();
}

void main() {
  testWidgets('adiciona código na tela vazia, normaliza e recarrega a lista', (
    tester,
  ) async {
    final salvos = <Map<String, Object?>>[];
    var posts = 0;
    final estado = await estadoPara(
      MockClient((request) async {
        if (request.url.path.endsWith('/conta/login')) return login();
        expect(request.headers['authorization'], 'Bearer token-teste');
        if (request.method == 'GET') return json(salvos);
        posts++;
        expect(jsonDecode(request.body), {'codigo': 'RESERVADO10'});
        final novo = cupom('RESERVADO10');
        salvos.add(novo);
        return json(novo);
      }),
    );
    await abrir(tester, estado);
    expect(find.text('Nenhum cupom disponível'), findsOneWidget);
    await tester.enterText(find.byType(TextFormField), 'reservado10');
    await tester.tap(find.widgetWithText(FilledButton, 'Adicionar cupom'));
    await tester.pumpAndSettle();
    expect(posts, 1);
    expect(find.text('RESERVADO10'), findsOneWidget);
    expect(find.text('Nenhum cupom disponível'), findsNothing);
    expect(find.text('Cupom adicionado.'), findsOneWidget);
  });

  testWidgets('rejeita campo vazio e mostra erro da API sem perder o código', (
    tester,
  ) async {
    var posts = 0;
    final estado = await estadoPara(
      MockClient((request) async {
        if (request.url.path.endsWith('/conta/login')) return login();
        if (request.method == 'GET') return json([]);
        posts++;
        return json({
          'detail': 'Cupom inválido, inativo ou fora da validade.',
        }, 400);
      }),
    );
    await abrir(tester, estado);
    await tester.tap(find.widgetWithText(FilledButton, 'Adicionar cupom'));
    await tester.pumpAndSettle();
    expect(posts, 0);
    expect(
      find.text('Informe um código com 3 a 24 caracteres.'),
      findsOneWidget,
    );
    await tester.enterText(find.byType(TextFormField), 'INVALIDO');
    await tester.tap(find.widgetWithText(FilledButton, 'Adicionar cupom'));
    await tester.pumpAndSettle();
    expect(posts, 1);
    expect(
      find.text('Cupom inválido, inativo ou fora da validade.'),
      findsOneWidget,
    );
    expect(find.text('INVALIDO'), findsOneWidget);
  });

  testWidgets('distingue cupom repetível, já utilizado e limite esgotado', (
    tester,
  ) async {
    final estado = await estadoPara(
      MockClient((request) async {
        if (request.url.path.endsWith('/conta/login')) return login();
        return json([
          cupom('REPETIVEL', umaVez: false, usado: true, usos: 1),
          cupom('UMA_VEZ', usado: true, usos: 1),
          cupom('ESGOTADO', umaVez: false, limite: 1, usos: 1),
        ]);
      }),
    );
    await abrir(tester, estado);
    expect(
      find.widgetWithText(OutlinedButton, 'Copiar código'),
      findsOneWidget,
    );
    expect(
      tester
          .widget<OutlinedButton>(
            find.widgetWithText(OutlinedButton, 'Copiar código'),
          )
          .onPressed,
      isNotNull,
    );
    expect(
      tester
          .widget<OutlinedButton>(
            find.widgetWithText(OutlinedButton, 'Já utilizado'),
          )
          .onPressed,
      isNull,
    );
    expect(
      tester
          .widget<OutlinedButton>(
            find.widgetWithText(OutlinedButton, 'Esgotado'),
          )
          .onPressed,
      isNull,
    );
  });
}
