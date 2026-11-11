using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class AdicionarCupomEntrada
    {
        [Required]
        [StringLength(24, MinimumLength = 3)]
        [RegularExpression("^[A-Za-z0-9_-]+$")]
        public string Codigo { get; set; } = "";
    }
}
