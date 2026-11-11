namespace PimBurguer.Api.Dtos
{
    public record ProdutoAdminResposta(
        int Id,
        string Nome,
        string Descricao,
        decimal Preco,
        string Categoria,
        string Imagem,
        bool Ativo,
        bool Removido);
}
