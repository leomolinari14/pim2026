namespace PimBurguer.Api.Services
{
    public class RegraException : Exception
    {
        public RegraException(string mensagem, int status = 400) : base(mensagem)
        {
            Status = status;
        }

        public int Status
        {
            get;
        }
    }
}
