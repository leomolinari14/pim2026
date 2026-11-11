using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class OrcamentoRequisicao
    {
        [Required]
        [MinLength(1)]
        [MaxLength(30)]
        public List<ItemSolicitado> Itens
        {
            get; set;
        } = new List<ItemSolicitado>()
        {
        };

        [Required]
        [RegularExpression("^(Retirada|Entrega)$")]
        public string Recebimento { get; set; } = "Retirada";

        [MaxLength(24)]
        public string? CodigoCupom
        {
            get; set;
        }
    }
}
