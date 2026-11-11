namespace PimBurguer.Api.Dtos
{
    public record PedidoResposta(
        Guid Id,
        DateTimeOffset CriadoEm,
        string Status,
        string NomeCliente,
        string Telefone,
        string Recebimento,
        string? Endereco,
        string Pagamento,
        decimal? TrocoPara,
        string? Observacoes,
        decimal Subtotal,
        decimal TaxaEntrega,
        decimal Total,
        List<ItemOrcamento> Itens,
        string WhatsAppUrl,
        decimal Desconto = 0,
        string? CodigoCupom = null,
        bool Pago = false,
        DateTimeOffset? PagoEm = null,
        Guid? UsuarioId = null,
        string? CpfCliente = null);
}
