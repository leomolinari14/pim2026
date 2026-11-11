using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class CupomEntrada
    {
        public int Id
        {
            get; set;
        }

        [Required]
        [StringLength(24, MinimumLength = 3)]
        [RegularExpression("^[A-Za-z0-9_-]+$")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = "";

        [Required]
        [RegularExpression("^(Percentual|ValorFixo)$")]
        [Display(Name = "Tipo de desconto")]
        public string Tipo { get; set; } = "Percentual";

        [Range(typeof(decimal), "0.01", "99999", ParseLimitsInInvariantCulture = true)]
        [Display(Name = "Valor do desconto")]
        public decimal Valor
        {
            get; set;
        }

        [Range(typeof(decimal), "0", "99999", ParseLimitsInInvariantCulture = true)]
        [Display(Name = "Compra mínima (R$)")]
        public decimal CompraMinima
        {
            get; set;
        }

        [DataType(DataType.Date)]
        [Display(Name = "Válido a partir de")]
        public DateOnly ValidoDe
        {
            get; set;
        }

        [DataType(DataType.Date)]
        [Display(Name = "Válido até")]
        public DateOnly ValidoAte
        {
            get; set;
        }

        [Range(1, 1000000)]
        [Display(Name = "Limite total de usos")]
        public int? LimiteUsos
        {
            get; set;
        }

        [Display(Name = "Uma utilização por cliente")]
        public bool UmaVezPorCliente { get; set; } = true;

        [Display(Name = "Mostrar no painel do cliente")]
        public bool Divulgar { get; set; } = true;

        [Display(Name = "Cupom ativo")]
        public bool Ativo { get; set; } = true;
    }
}
