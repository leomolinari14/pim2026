using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PimBurguer.Validacao
{
    public sealed class CpfAttribute : ValidationAttribute
    {
        public CpfAttribute() : base("Informe um CPF válido.")
        {
        }

        public override bool IsValid(object? value)
        {
            return value is null || value is string texto && (string.IsNullOrWhiteSpace(texto) || Cpf.Valido(texto));
        }
    }
}
