namespace PimBurguer.Api.Models
{
    public class UsoCupom
    {
        public int Id
        {
            get; set;
        }
        public int CupomId
        {
            get; set;
        }
        public Cupom Cupom { get; set; } = null!;
        public Guid UsuarioId
        {
            get; set;
        }
        public Usuario Usuario { get; set; } = null!;
        public Guid PedidoId
        {
            get; set;
        }
        public Pedido Pedido { get; set; } = null!;
    }
}
