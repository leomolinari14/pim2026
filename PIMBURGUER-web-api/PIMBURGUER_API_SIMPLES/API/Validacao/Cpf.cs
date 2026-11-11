using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PimBurguer.Validacao
{
    public static class Cpf
    {
        public static string Normalizar(string? valor)
        {
            return Regex.Replace(valor ?? "", "[^0-9]", "");
        }

        public static bool Valido(string? valor)
        {
            if (!Regex.IsMatch((valor ?? "").Trim(), @"^([0-9]{11}|[0-9]{3}\.[0-9]{3}\.[0-9]{3}-[0-9]{2})$"))
            {
                return false;
            }

            var numeros = Normalizar(valor);
            if (numeros.All(n => n == numeros[0]))
            {
                return false;
            }

            for (var tamanho = 9; tamanho <= 10; tamanho++)
            {
                var soma = 0;
                for (var i = 0; i < tamanho; i++)
                {
                    soma += (numeros[i] - '0') * (tamanho + 1 - i);
                }

                var resto = soma % 11;
                if ((resto < 2 ? 0 : 11 - resto) != numeros[tamanho] - '0')
                {
                    return false;
                }
            }

            return true;
        }

        public static string Formatar(string? valor)
        {
            var numeros = Normalizar(valor);
            if (numeros.Length == 11)
            {
                return $"{numeros[..3]}.{numeros[3..6]}.{numeros[6..9]}-{numeros[9..]}";
            }
            else
            {
                return "";
            }
        }
    }
}
