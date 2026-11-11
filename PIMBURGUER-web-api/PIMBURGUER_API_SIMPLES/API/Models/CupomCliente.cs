namespace PimBurguer.Api.Models
{
    public class CupomCliente
    {
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public int CupomId { get; set; }
        public Cupom Cupom { get; set; } = null!;
    }
}
