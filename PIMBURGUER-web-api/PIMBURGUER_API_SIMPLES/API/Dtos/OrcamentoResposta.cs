namespace PimBurguer.Api.Dtos
{
    public record OrcamentoResposta(
        List<ItemOrcamento> Itens,
        string Recebimento,
        decimal Subtotal,
        decimal TaxaEntrega,
        decimal Total,
        string Hash,
        decimal Desconto = 0,
        string? CodigoCupom = null,
        int? CupomId = null);
}
