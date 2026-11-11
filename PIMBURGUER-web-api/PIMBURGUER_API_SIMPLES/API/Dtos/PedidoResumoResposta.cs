namespace PimBurguer.Api.Dtos
{
    public record PedidoResumoResposta(
        Guid Id,
        DateTimeOffset CriadoEm,
        string NomeCliente,
        string Status,
        string Recebimento,
        string Pagamento,
        bool Pago,
        decimal Total,
        decimal Desconto,
        string? CodigoCupom,
        int QuantidadeItens);
}
