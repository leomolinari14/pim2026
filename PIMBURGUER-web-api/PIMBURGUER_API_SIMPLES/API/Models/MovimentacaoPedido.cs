namespace PimBurguer.Api.Models
{
    public class MovimentacaoPedido
    {
        public int Id
        {
            get; set;
        }
        public Guid PedidoId
        {
            get; set;
        }
        public Pedido Pedido { get; set; } = null!;
        public Guid AdministradorId
        {
            get; set;
        }
        public Usuario Administrador { get; set; } = null!;
        public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
        public string StatusAnterior { get; set; } = "";
        public string StatusNovo { get; set; } = "";
        public bool PagoAnterior
        {
            get; set;
        }
        public bool PagoNovo
        {
            get; set;
        }
    }
}
