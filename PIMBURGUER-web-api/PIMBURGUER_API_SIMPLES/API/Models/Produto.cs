using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Models
{
    // Cada objeto Produto corresponde a uma linha da tabela produtos.
    public class Produto
    {
        public int Id
        {
            get; set;
        }

        [Required]
        [MaxLength(120)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true)]
        public decimal Preco
        {
            get; set;
        }

        [Required]
        [MaxLength(40)]
        public string Categoria { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Imagem { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public bool Removido
        {
            get; set;
        }
    }
}
