namespace PimBurguer.Api.Dtos
{
    public record CupomResposta(
        int Id,
        string Codigo,
        string Tipo,
        decimal Valor,
        decimal CompraMinima,
        DateOnly ValidoDe,
        DateOnly ValidoAte,
        int? LimiteUsos,
        bool UmaVezPorCliente,
        bool Divulgar,
        bool Ativo,
        int Usos,
        bool UtilizadoPeloCliente);
}
