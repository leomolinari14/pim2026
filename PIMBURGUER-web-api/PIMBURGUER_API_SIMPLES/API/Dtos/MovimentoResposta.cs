namespace PimBurguer.Api.Dtos
{
    public record MovimentoResposta(
        DateTimeOffset CriadoEm,
        string Administrador,
        string StatusAnterior,
        string StatusNovo,
        bool PagoAnterior,
        bool PagoNovo);
}
