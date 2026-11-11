namespace PimBurguer.Api.Services
{
    public class PedidoInvalidoException : Exception
    {
        public PedidoInvalidoException(string mensagem, int status = 400) : base(mensagem)
        {
            Status = status;
        }

        public int Status
        {
            get;
        }
    }
}
