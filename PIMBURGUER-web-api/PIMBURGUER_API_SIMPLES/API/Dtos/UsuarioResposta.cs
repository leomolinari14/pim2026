namespace PimBurguer.Api.Dtos
{
    public record UsuarioResposta(
        Guid Id,
        string Nome,
        string? Email,
        string Telefone,
        string Perfil,
        string? NomeUsuario,
        string? Cpf = null);
}
