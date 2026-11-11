import '../../models/produto.dart';
import '../api/produto_api.dart';
import '../db/app_database.dart';

class ProdutoRepository {
  final ProdutoApi api;
  final AppDatabase banco;

  const ProdutoRepository(this.api, this.banco);

  Future<List<Produto>> listarLocal() => banco.lerProdutos();

  Future<List<Produto>> atualizar() async {
    final produtos = await api.buscarCatalogo();
    await banco.salvarProdutos(produtos);
    return produtos;
  }
}
