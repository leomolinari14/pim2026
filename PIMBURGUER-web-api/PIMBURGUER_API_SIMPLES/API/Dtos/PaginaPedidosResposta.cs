namespace PimBurguer.Api.Dtos
{
    public record PaginaPedidosResposta(List<PedidoResumoResposta> Pedidos, int Pagina, int Total, int Tamanho)
    {
        public int TotalPaginas
        {
            get
            {
                return Math.Max(1, (int)Math.Ceiling((double)Total / Tamanho));
            }
        }
    }
}
