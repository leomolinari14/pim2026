using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class AtualizarPedidoEntrada
    {
        [Required]
        [MaxLength(40)]
        public string Status { get; set; } = "Aguardando confirmação";
        public bool Pago
        {
            get; set;
        }
    }
}
