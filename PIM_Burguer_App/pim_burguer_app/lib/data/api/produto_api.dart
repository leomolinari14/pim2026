import '../../models/pedido.dart';
import '../../models/produto.dart';
import 'api_cliente.dart';

class ProdutoApi {
  final ApiCliente cliente;
  const ProdutoApi(this.cliente);

  Future<List<Produto>> buscarCatalogo() async =>
      listaDe(await cliente.enviar('GET', 'api/produtos'), Produto.fromApi);
}
