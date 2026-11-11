namespace PimBurguer.Api.Dtos
{
    public record RelatorioResposta(
        DateOnly Inicio,
        DateOnly Fim,
        int TotalPedidos,
        int PedidosPagos,
        int PedidosCancelados,
        decimal ValorPedidos,
        decimal ValorRecebido,
        decimal Descontos,
        decimal TicketMedio,
        List<SerieDiaResposta> Dias,
        List<ProdutoVendidoResposta> Produtos,
        List<ContagemResposta> Status);
}
