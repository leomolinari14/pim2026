namespace PimBurguer.Api.Dtos
{
    // A API devolve apenas os dados que o cardápio precisa exibir.
    public record ProdutoResposta(
        int Id,
        string Nome,
        string Descricao,
        decimal Preco,
        string Categoria,
        string Imagem);
}
