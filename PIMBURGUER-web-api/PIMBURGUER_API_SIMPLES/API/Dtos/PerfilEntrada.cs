using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class PerfilEntrada
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Nome completo")]
        public string Nome { get; set; } = "";

        [Required]
        [MaxLength(20)]
        [Display(Name = "Telefone com DDD")]
        public string Telefone { get; set; } = "";
    }
}
