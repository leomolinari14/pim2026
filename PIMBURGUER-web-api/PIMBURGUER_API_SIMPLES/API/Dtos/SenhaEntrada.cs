using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class SenhaEntrada
    {
        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string SenhaAtual { get; set; } = "";

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,100}$", ErrorMessage = "Use ao menos 8 caracteres, com letras e números.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova senha")]
        public string NovaSenha { get; set; } = "";

        [Required]
        [Compare(nameof(NovaSenha))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nova senha")]
        public string ConfirmarSenha { get; set; } = "";
    }
}
