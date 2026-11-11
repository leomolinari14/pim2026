namespace PimBurguer.Api.Dtos
{
    public record DetalheAdminResposta(PedidoResposta Pedido, List<MovimentoResposta> Movimentos);
}
