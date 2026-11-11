namespace PimBurguer.Api.Models
{
    public class ItemPedido
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
        public int ProdutoId
        {
            get; set;
        }
        public Produto Produto { get; set; } = null!;
        // Nome e preço ficam registrados como eram no momento da compra.
        public string NomeProduto { get; set; } = "";
        public int Quantidade
        {
            get; set;
        }
        public decimal PrecoUnitario
        {
            get; set;
        }
        public decimal Subtotal
        {
            get; set;
        }
    }
}
