import 'dart:convert';

import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:pim_burguer_app/core/app_estado.dart';
import 'package:pim_burguer_app/data/api/api_cliente.dart';
import 'package:pim_burguer_app/data/api/conta_api.dart';
import 'package:pim_burguer_app/data/api/pedido_api.dart';
import 'package:pim_burguer_app/data/api/produto_api.dart';
import 'package:pim_burguer_app/data/db/app_database.dart';
import 'package:pim_burguer_app/data/repositories/pedido_repository.dart';
import 'package:pim_burguer_app/data/repositories/produto_repository.dart';

const _canal = MethodChannel('pim_burguer/armazenamento');
const _produto = {
  'id': 1,
  'nome': 'Pim Clássico',
  'descricao': 'Hambúrguer da casa',
  'preco': 19.90,
  'categoria': 'Hambúrgueres',
  'imagem': '/imagens/produto-classico.png',
};

http.Response _json(Object dados, [int status = 200]) => http.Response(
  jsonEncode(dados),
  status,
  headers: {'content-type': 'application/json; charset=utf-8'},
);

Map<String, Object?> _orcamento() => {
  'itens': [
    {'produtoId': 1, 'nome': 'Pim Clássico', 'quantidade': 1, 'subtotal': 19.9},
  ],
  'subtotal': 19.9,
  'taxaEntrega': 5,
  'total': 24.9,
  'desconto': 0,
  'hash': 'A' * 64,
};

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  final messenger =
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger;
  late Map<int, int> carrinho;
  late List<Object?> cache;
  late Map<String, String> pendencias;

  setUp(() {
    carrinho = {};
    cache = [];
    pendencias = {};
    messenger.setMockMethodCallHandler(_canal, (call) async {
      final args = call.arguments as Map;
      switch (call.method) {
        case 'lerProdutos':
          return cache;
        case 'salvarProdutos':
          cache = List<Object?>.from(args['produtos'] as List);
        case 'lerCarrinho':
          return carrinho;
        case 'salvarCarrinho':
          carrinho = Map<int, int>.from(args['itens'] as Map);
        case 'lerPendencia':
          return pendencias[args['usuario']];
        case 'salvarPendencia':
          pendencias[args['usuario'] as String] = args['json'] as String;
        case 'excluirPendencia':
          pendencias.remove(args['usuario']);
        default:
          throw StateError(call.method);
      }
      return null;
    });
  });

  tearDown(() => messenger.setMockMethodCallHandler(_canal, null));

  test(
    'recupera um envio após reiniciar usando o mesmo corpo e limpa o carrinho',
    () async {
      final chamadas = <String>[];
      final enviados = <Map<String, dynamic>>[];

      Future<http.Response> responder(http.Request req) async {
        final rota = '${req.method} ${req.url.path}';
        chamadas.add(rota);
        switch (rota) {
          case 'GET /api/produtos':
            return _json([_produto]);
          case 'POST /api/conta/login':
            return _json({
              'token': 'jwt-de-teste',
              'expiraEm': DateTime.now()
                  .add(const Duration(minutes: 30))
                  .toIso8601String(),
              'usuario': {
                'id': 'cliente-teste',
                'nome': 'Cliente teste',
                'email': 'teste@example.test',
                'telefone': '11999999999',
                'perfil': 'Cliente',
                'cpf': '52998224725',
              },
            });
          case 'POST /api/pedidos/orcamento':
            expect(req.headers['Authorization'], 'Bearer jwt-de-teste');
            final dados = jsonDecode(req.body) as Map;
            expect(dados['recebimento'], 'Entrega');
            expect(dados.containsKey('total'), isFalse);
            return _json(_orcamento());
          case 'POST /api/pedidos':
            expect(req.headers['Authorization'], 'Bearer jwt-de-teste');
            final dados = jsonDecode(req.body) as Map<String, dynamic>;
            enviados.add(dados);
            if (enviados.length == 1) {
              throw http.ClientException('conexão interrompida');
            }
            return _json({
              ..._orcamento(),
              'id': dados['id'],
              'criadoEm': '2026-11-08T20:00:00Z',
              'status': 'Aguardando confirmação',
              'recebimento': 'Entrega',
              'pagamento': 'Pix',
              'pago': false,
              'whatsAppUrl': 'https://wa.me/',
            }, 201);
          default:
            throw StateError(rota);
        }
      }

      AppEstado criar() {
        final api = ApiCliente(
          Uri.parse('https://pim.example.test/'),
          httpClient: MockClient(responder),
        );
        const banco = AppDatabase('https://pim.example.test/');
        return AppEstado(
          api,
          ContaApi(api),
          ProdutoRepository(ProdutoApi(api), banco),
          PedidoRepository(PedidoApi(api), banco),
          banco,
        );
      }

      final primeiro = criar();
      addTearDown(primeiro.dispose);
      await primeiro.iniciar();
      expect(primeiro.produtos.single.precoCentavos, 1990);
      await primeiro.mudarQuantidade(1, 1);
      await primeiro.autenticar(false, {
        'email': 'teste@example.test',
        'senha': 'senha-teste',
      });
      final dados = <String, Object?>{
        'itens': [
          {'produtoId': 1, 'quantidade': 1},
        ],
        'recebimento': 'Entrega',
      };
      final resumo = await primeiro.orcar(dados);
      expect(resumo.total, 24.9);
      await expectLater(
        primeiro.confirmar({...dados, 'orcamentoHash': resumo.hash}),
        throwsA(isA<ApiErro>()),
      );
      expect(await primeiro.temPendencia(), isTrue);
      expect(carrinho, {1: 1});

      final segundo = criar();
      addTearDown(segundo.dispose);
      await segundo.iniciar();
      expect(segundo.carrinho, {1: 1});
      await segundo.autenticar(false, {
        'email': 'teste@example.test',
        'senha': 'senha-teste',
      });
      expect(await segundo.temPendencia(), isTrue);
      final pedido = await segundo.confirmar();
      expect(pedido.id, enviados.first['id']);
      expect(enviados.last, enviados.first);
      expect(enviados.last['tokenConsulta'], hasLength(44));
      expect(carrinho, isEmpty);
      expect(pendencias, isEmpty);
      expect(chamadas.where((e) => e == 'POST /api/pedidos'), hasLength(2));
    },
  );
}
