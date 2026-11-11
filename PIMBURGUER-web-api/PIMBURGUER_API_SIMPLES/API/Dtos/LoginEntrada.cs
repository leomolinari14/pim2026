using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class LoginEntrada
    {
        [Required]
        [MaxLength(180)]
        [Display(Name = "Usuário ou e-mail")]
        public string Email { get; set; } = "";

        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = "";
    }
}
