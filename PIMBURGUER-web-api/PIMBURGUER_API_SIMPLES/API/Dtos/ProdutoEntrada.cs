using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class ProdutoEntrada
    {
        public int Id
        {
            get; set;
        }

        [Required]
        [StringLength(120, MinimumLength = 2)]
        [Display(Name = "Nome do produto")]
        public string Nome { get; set; } = "";

        [Required]
        [MaxLength(500)]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = "";

        [Range(typeof(decimal), "0.01", "99999", ParseLimitsInInvariantCulture = true)]
        [Display(Name = "Preço (R$)")]
        public decimal Preco
        {
            get; set;
        }

        [Required]
        [MaxLength(40)]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; } = "Hambúrgueres";

        [Display(Name = "Disponível no cardápio")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Imagem do produto")]
        public IFormFile? ArquivoImagem
        {
            get; set;
        }
        public string? ImagemAtual
        {
            get; set;
        }
    }
}
