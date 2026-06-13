using System.Net;

namespace FIAP.NotificationsAPI.Application.DTOs.Responses;

public class ExceptionOutputDto
{
    public HttpStatusCode? CodigoStatus { get; set; }
    public string? Mensagem { get; set; }
    public string? MensagemInterna { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerExceptionMessage { get; set; }
    public List<dynamic>? ListaErros { get; set; }
    public string? TipoErro { get; set; }
    public string? TraceId { get; set; }
    public string? CorrelationId { get; set; }
    public string? Caminho { get; set; }
    public string? Metodo { get; set; }
    public DateTimeOffset? DataHoraUtc { get; set; } = DateTimeOffset.UtcNow;

    public ExceptionOutputDto()
    {
    }

    public ExceptionOutputDto(Exception ex)
    {
        CodigoStatus = HttpStatusCode.InternalServerError;
        Mensagem = "Ocorreu um erro inesperado.";
        TipoErro = ex.GetType().Name;
        DataHoraUtc = DateTimeOffset.UtcNow;
        MensagemInterna = ex.Message;
        StackTrace = ex.StackTrace;
        InnerExceptionMessage = ex.InnerException?.Message;
    }
}
