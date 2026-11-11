namespace PimBurguer.Api.Models
{
    public class Pedido
    {
        // O mesmo identificador é reaproveitado em uma tentativa repetida de envio.
        public Guid Id
        {
            get; set;
        }
        public DateTimeOffset CriadoEm
        {
            get; set;
        }
        public string Status { get; set; } = "Aguardando confirmação";
        public Guid? UsuarioId
        {
            get; set;
        }
        public int? CupomId
        {
            get; set;
        }
        public string? CodigoCupom
        {
            get; set;
        }
        public decimal Desconto
        {
            get; set;
        }
        public bool Pago
        {
            get; set;
        }
        public DateTimeOffset? PagoEm
        {
            get; set;
        }
        public string NomeCliente { get; set; } = "";
        public string Telefone { get; set; } = "";
        public string? CpfCliente
        {
            get; set;
        }
        public string Recebimento { get; set; } = "";
        public string? Cep
        {
            get; set;
        }
        public string? Logradouro
        {
            get; set;
        }
        public string? Numero
        {
            get; set;
        }
        public string? Complemento
        {
            get; set;
        }
        public string? Bairro
        {
            get; set;
        }
        public string? Cidade
        {
            get; set;
        }
        public string? Uf
        {
            get; set;
        }
        public string Pagamento { get; set; } = "";
        public decimal? TrocoPara
        {
            get; set;
        }
        public string? Observacoes
        {
            get; set;
        }
        public decimal Subtotal
        {
            get; set;
        }
        public decimal TaxaEntrega
        {
            get; set;
        }
        public decimal Total
        {
            get; set;
        }
        public string WhatsAppLoja { get; set; } = "";
        // Só o hash do segredo é persistido. Ele protege a consulta do pedido.
        public string TokenHash { get; set; } = "";
        public string RequisicaoHash { get; set; } = "";
        public List<ItemPedido> Itens
        {
            get; set;
        } = new List<ItemPedido>()
        {
        };
    }
}
