namespace FIAP.NotificationsAPI.Domain.Exceptions;

public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message)
        : base(message)
    {
    }
}
