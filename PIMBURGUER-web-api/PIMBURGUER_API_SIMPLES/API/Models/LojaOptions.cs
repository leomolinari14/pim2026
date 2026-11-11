namespace PimBurguer.Api.Models
{
    public class LojaOptions
    {
        public string Nome { get; set; } = "PimBurguer";
        public string WhatsApp { get; set; } = "";
        public decimal? TaxaEntrega
        {
            get; set;
        }
        public string OrientacaoRetirada { get; set; } = "";
    }
}
