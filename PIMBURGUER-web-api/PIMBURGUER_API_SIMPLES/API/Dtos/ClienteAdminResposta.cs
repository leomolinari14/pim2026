namespace PimBurguer.Api.Dtos
{
    public record ClienteAdminResposta(
        Guid Id,
        string Nome,
        string Email,
        string Telefone,
        bool Ativo,
        DateTimeOffset CriadoEm,
        int Pedidos);
}
