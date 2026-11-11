using System.ComponentModel.DataAnnotations;
using PimBurguer.Validacao;

namespace PimBurguer.Api.Dtos
{
    public class CadastroEntrada
    {
        [Required(ErrorMessage = "Informe o CPF.")]
        [MaxLength(14)]
        [CpfAttribute]
        [Display(Name = "CPF do cliente")]
        public string Cpf { get; set; } = "";

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Nome completo")]
        public string Nome { get; set; } = "";

        [Required]
        [EmailAddress]
        [MaxLength(180)]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = "";

        [Required]
        [MaxLength(20)]
        [Display(Name = "Telefone com DDD")]
        public string Telefone { get; set; } = "";

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,100}$", ErrorMessage = "Use ao menos 8 caracteres, com letras e números.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = "";

        [Required]
        [Compare(nameof(Senha), ErrorMessage = "As senhas precisam ser iguais.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha")]
        public string ConfirmarSenha { get; set; } = "";
    }
}
