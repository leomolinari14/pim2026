using System.ComponentModel.DataAnnotations;
using PimBurguer.Validacao;

namespace PimBurguer.Api.Dtos
{
    public class PedidoRequisicao : OrcamentoRequisicao
    {
        [Required(ErrorMessage = "Informe o CPF.")]
        [MaxLength(14)]
        [CpfAttribute]
        [Display(Name = "CPF do cliente")]
        public string CpfCliente { get; set; } = "";
        public Guid Id
        {
            get; set;
        }

        [Required]
        [StringLength(128, MinimumLength = 32)]
        public string TokenConsulta { get; set; } = "";

        [Required]
        [StringLength(64, MinimumLength = 64)]
        public string OrcamentoHash { get; set; } = "";

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string NomeCliente { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string Telefone { get; set; } = "";

        [MaxLength(9)]
        public string? Cep
        {
            get; set;
        }

        [MaxLength(160)]
        public string? Logradouro
        {
            get; set;
        }

        [MaxLength(12)]
        public string? Numero
        {
            get; set;
        }

        [MaxLength(80)]
        public string? Complemento
        {
            get; set;
        }

        [MaxLength(80)]
        public string? Bairro
        {
            get; set;
        }

        [MaxLength(80)]
        public string? Cidade
        {
            get; set;
        }

        [MaxLength(2)]
        public string? Uf
        {
            get; set;
        }

        [Required]
        [RegularExpression("^(Pix|Dinheiro|CartaoDebito|CartaoCredito)$")]
        public string Pagamento { get; set; } = "Pix";

        [Range(typeof(decimal), "0.01", "100000", ParseLimitsInInvariantCulture = true)]
        public decimal? TrocoPara
        {
            get; set;
        }

        [MaxLength(500)]
        public string? Observacoes
        {
            get; set;
        }
    }
}
