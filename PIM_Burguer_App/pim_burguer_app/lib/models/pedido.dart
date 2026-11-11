Map<String, dynamic> objeto(Object? valor) => valor as Map<String, dynamic>;

List<T> listaDe<T>(Object? valor, T Function(Map<String, dynamic>) ler) =>
    (valor as List).map((item) => ler(objeto(item))).toList();

List<Map<String, int>> itensSolicitados(Map<int, int> itens) => [
  for (final item in itens.entries)
    {'produtoId': item.key, 'quantidade': item.value},
];

Map<int, int> quantidades(Object? json) => Map.fromEntries(
  listaDe(json, (i) => MapEntry(i['produtoId'] as int, i['quantidade'] as int)),
);

class ItemPedido {
  final String nome;
  final int quantidade;
  final num subtotal;

  ItemPedido.fromJson(Map<String, dynamic> json)
    : nome = json['nome'] as String,
      quantidade = json['quantidade'] as int,
      subtotal = json['subtotal'] as num;
}

class ValoresPedido {
  final List<ItemPedido> itens;
  final num subtotal;
  final num taxaEntrega;
  final num desconto;
  final num total;
  final String? codigoCupom;

  ValoresPedido.fromJson(Map<String, dynamic> json)
    : itens = listaDe(json['itens'], ItemPedido.fromJson),
      subtotal = json['subtotal'] as num,
      taxaEntrega = json['taxaEntrega'] as num,
      desconto = json['desconto'] as num? ?? 0,
      total = json['total'] as num,
      codigoCupom = json['codigoCupom'] as String?;
}

class Orcamento extends ValoresPedido {
  final String hash;

  Orcamento.fromJson(super.json)
    : hash = json['hash'] as String,
      super.fromJson();
}

class Pedido extends ValoresPedido {
  final String id;
  final DateTime criadoEm;
  final String status;
  final String recebimento;
  final String pagamento;
  final bool pago;
  final String? endereco;
  final String? observacoes;
  final num? trocoPara;
  final String whatsAppUrl;

  Pedido.fromJson(super.json)
    : id = json['id'] as String,
      criadoEm = DateTime.parse(json['criadoEm'] as String),
      status = json['status'] as String,
      recebimento = json['recebimento'] as String,
      pagamento = json['pagamento'] as String,
      pago = json['pago'] as bool,
      endereco = json['endereco'] as String?,
      observacoes = json['observacoes'] as String?,
      trocoPara = json['trocoPara'] as num?,
      whatsAppUrl = json['whatsAppUrl'] as String? ?? '',
      super.fromJson();
}

class PedidoResumo {
  final String id;
  final DateTime criadoEm;
  final String status;
  final num total;
  final int quantidadeItens;

  PedidoResumo.fromJson(Map<String, dynamic> json)
    : id = json['id'] as String,
      criadoEm = DateTime.parse(json['criadoEm'] as String),
      status = json['status'] as String,
      total = json['total'] as num,
      quantidadeItens = json['quantidadeItens'] as int;
}

class PaginaPedidos {
  final List<PedidoResumo> pedidos;
  final int pagina;
  final int totalPaginas;

  PaginaPedidos.fromJson(Map<String, dynamic> json)
    : pedidos = listaDe(json['pedidos'], PedidoResumo.fromJson),
      pagina = json['pagina'] as int,
      totalPaginas = json['totalPaginas'] as int;
}

class RepeticaoPedido {
  final Map<int, int> itens;
  final List<String> indisponiveis;

  RepeticaoPedido.fromJson(Map<String, dynamic> json)
    : itens = quantidades(json['itens']),
      indisponiveis = List<String>.from(json['indisponiveis'] as List);
}

class Cupom {
  final String codigo;
  final bool percentual;
  final num valor;
  final num compraMinima;
  final String validoAte;
  final bool utilizado;
  final bool esgotado;

  Cupom.fromJson(Map<String, dynamic> json)
    : codigo = json['codigo'] as String,
      percentual = json['tipo'] == 'Percentual',
      valor = json['valor'] as num,
      compraMinima = json['compraMinima'] as num,
      validoAte = json['validoAte'] as String,
      utilizado =
          (json['umaVezPorCliente'] as bool? ?? true) &&
          (json['utilizadoPeloCliente'] as bool),
      esgotado =
          json['limiteUsos'] != null &&
          (json['usos'] as num? ?? 0) >= (json['limiteUsos'] as num);
}
