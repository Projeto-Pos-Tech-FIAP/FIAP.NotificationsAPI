using System.Diagnostics;
using System.Net;
using FIAP.NotificationsAPI.Application.DTOs.Responses;
using FIAP.NotificationsAPI.Domain.Exceptions;

namespace FIAP.NotificationsAPI.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");

            if (context.Response.HasStarted)
                throw;

            var (statusCode, error) = MapException(ex);
            EnrichException(context, ex, error);

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(error);
        }
    }

    private static (HttpStatusCode statusCode, ExceptionOutputDto error) MapException(Exception ex)
    {
        return ex switch
        {
            DomainException domain => (HttpStatusCode.BadRequest, CreateExceptionDto(domain, HttpStatusCode.BadRequest)),
            OperationCanceledException canceled => (HttpStatusCode.BadRequest, CreateExceptionDto(canceled, HttpStatusCode.BadRequest)),
            ArgumentException argument => (HttpStatusCode.BadRequest, CreateExceptionDto(argument, HttpStatusCode.BadRequest)),
            HttpRequestException httpRequest => MapHttpRequestException(httpRequest),
            _ => (HttpStatusCode.InternalServerError, new ExceptionOutputDto(ex))
        };
    }

    private static (HttpStatusCode statusCode, ExceptionOutputDto error) MapHttpRequestException(HttpRequestException ex)
    {
        var httpStatusCode = ex.StatusCode ?? HttpStatusCode.BadGateway;
        var exception = new ExceptionOutputDto(ex);
        exception.CodigoStatus ??= httpStatusCode;
        return (httpStatusCode, exception);
    }

    private static ExceptionOutputDto CreateExceptionDto(Exception ex, HttpStatusCode statusCode)
    {
        return new ExceptionOutputDto
        {
            CodigoStatus = statusCode,
            Mensagem = ex.Message,
            MensagemInterna = ex.Message,
            StackTrace = ex.StackTrace,
            InnerExceptionMessage = ex.InnerException?.Message
        };
    }

    private static void EnrichException(HttpContext context, Exception ex, ExceptionOutputDto error)
    {
        error.TipoErro ??= ex.GetType().Name;
        error.DataHoraUtc ??= DateTimeOffset.UtcNow;

        string? traceId = Activity.Current?.TraceId.ToString();
        error.TraceId ??= string.IsNullOrWhiteSpace(traceId) ? context.TraceIdentifier : traceId;
        error.CorrelationId ??= GetCorrelationId(context);
        error.Caminho ??= context.Request.Path.Value;
        error.Metodo ??= context.Request.Method;
    }

    private static string? GetCorrelationId(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = context.Request.Headers["X-Request-ID"].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = context.Request.Headers["Correlation-ID"].ToString();

        return string.IsNullOrWhiteSpace(correlationId) ? null : correlationId;
    }
}
