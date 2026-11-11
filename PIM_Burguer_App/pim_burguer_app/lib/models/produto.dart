class Produto {
  final int id;
  final String nome;
  final String descricao;
  final int precoCentavos;
  final String categoria;
  final String imagem;

  const Produto(
    this.id,
    this.nome,
    this.descricao,
    this.precoCentavos,
    this.categoria,
    this.imagem,
  );

  factory Produto.fromApi(Map<String, dynamic> json) => Produto(
    json['id'] as int,
    json['nome'] as String,
    json['descricao'] as String,
    ((json['preco'] as num) * 100).round(),
    json['categoria'] as String,
    json['imagem'] as String,
  );

  factory Produto.fromLocal(Map<dynamic, dynamic> json) => Produto(
    json['id'] as int,
    json['nome'] as String,
    json['descricao'] as String,
    json['precoCentavos'] as int,
    json['categoria'] as String,
    json['imagem'] as String,
  );

  Map<String, Object> toLocal() => {
    'id': id,
    'nome': nome,
    'descricao': descricao,
    'precoCentavos': precoCentavos,
    'categoria': categoria,
    'imagem': imagem,
  };
}
