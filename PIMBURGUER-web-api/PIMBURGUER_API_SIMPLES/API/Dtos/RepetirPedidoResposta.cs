namespace PimBurguer.Api.Dtos
{
    public record RepetirPedidoResposta(List<ItemSolicitado> Itens, List<string> Indisponiveis);
}
