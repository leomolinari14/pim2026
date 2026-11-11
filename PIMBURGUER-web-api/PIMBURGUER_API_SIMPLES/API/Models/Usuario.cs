namespace PimBurguer.Api.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = "";
        public string? NomeUsuario
        {
            get; set;
        }
        public string? Email
        {
            get; set;
        }
        public string? EmailNormalizado
        {
            get; set;
        }
        public string Telefone { get; set; } = "";
        public string? Cpf
        {
            get; set;
        }
        public string SenhaHash { get; set; } = "";
        public string Perfil { get; set; } = "Cliente";
        public bool Ativo { get; set; } = true;
        public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
        public int FalhasLogin
        {
            get; set;
        }
        public DateTimeOffset? BloqueadoAte
        {
            get; set;
        }
    }
}
