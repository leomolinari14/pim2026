import '../../models/pedido.dart';
import 'api_cliente.dart';

class PedidoApi {
  final ApiCliente cliente;
  const PedidoApi(this.cliente);

  Future<Orcamento> orcar(String token, Map<String, Object?> dados) async =>
      Orcamento.fromJson(
        objeto(
          await cliente.enviar(
            'POST',
            'api/pedidos/orcamento',
            token: token,
            corpo: dados,
          ),
        ),
      );

  Future<Pedido> enviar(String token, Map<String, Object?> dados) async =>
      Pedido.fromJson(
        objeto(
          await cliente.enviar(
            'POST',
            'api/pedidos',
            token: token,
            corpo: dados,
          ),
        ),
      );

  Future<PaginaPedidos> listar(String token, int pagina) async =>
      PaginaPedidos.fromJson(
        objeto(
          await cliente.enviar(
            'GET',
            'api/cliente/pedidos',
            token: token,
            consulta: {'pagina': '$pagina'},
          ),
        ),
      );

  Future<Pedido> detalhar(String token, String id) async => Pedido.fromJson(
    objeto(
      await cliente.enviar('GET', 'api/cliente/pedidos/$id', token: token),
    ),
  );

  Future<RepeticaoPedido> repetir(String token, String id) async =>
      RepeticaoPedido.fromJson(
        objeto(
          await cliente.enviar(
            'POST',
            'api/cliente/pedidos/$id/repetir',
            token: token,
          ),
        ),
      );

  Future<List<Cupom>> cupons(String token) async => listaDe(
    await cliente.enviar('GET', 'api/cliente/cupons', token: token),
    Cupom.fromJson,
  );

  Future<Cupom> adicionarCupom(String token, String codigo) async =>
      Cupom.fromJson(
        objeto(
          await cliente.enviar(
            'POST',
            'api/cliente/cupons',
            token: token,
            corpo: {'codigo': codigo},
          ),
        ),
      );
}
