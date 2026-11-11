namespace PimBurguer.Api.Dtos
{
    public record ItemOrcamento(
        int ProdutoId,
        string Nome,
        string Imagem,
        int Quantidade,
        decimal PrecoUnitario,
        decimal Subtotal);
}
