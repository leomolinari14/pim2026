namespace PimBurguer.Api.Models
{
    public class SessaoAcesso
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId
        {
            get; set;
        }
        public Usuario Usuario { get; set; } = null!;
        public string TokenHash { get; set; } = "";
        public DateTimeOffset ExpiraEm
        {
            get; set;
        }
    }
}
