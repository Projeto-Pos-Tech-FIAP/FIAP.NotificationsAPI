using System.ComponentModel.DataAnnotations;
using FIAP.NotificationsAPI.Domain.Enums;

namespace FIAP.NotificationsAPI.Application.DTOs.Requests;

public class SendPaymentProcessedEmailRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int GameId { get; set; }

    public string? CorrelationId { get; set; }

    public PaymentStatus? Status { get; set; }
}
