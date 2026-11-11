namespace PimBurguer.Api.Models
{
    public class Cupom
    {
        // Apenas no banco mobile: vínculo com o cupom oficial do banco web.
        public int? CupomCompartilhadoId { get; set; }
        public int Id
        {
            get; set;
        }
        public string Codigo { get; set; } = "";
        public string Tipo { get; set; } = "Percentual";
        public decimal Valor
        {
            get; set;
        }
        public decimal CompraMinima
        {
            get; set;
        }
        public DateOnly ValidoDe
        {
            get; set;
        }
        public DateOnly ValidoAte
        {
            get; set;
        }
        public int? LimiteUsos
        {
            get; set;
        }
        public bool UmaVezPorCliente { get; set; } = true;
        public bool Divulgar { get; set; } = true;
        public bool Ativo { get; set; } = true;
    }
}
