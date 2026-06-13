using System.ComponentModel.DataAnnotations;
using FIAP.NotificationsAPI.Domain.Enums;

namespace FIAP.NotificationsAPI.Application.DTOs.Requests;

public class SendPaymentProcessedEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public int? OrderId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal PaymentAmount { get; set; }

    public DateTime PaymentDate { get; set; }

    public PaymentStatus? Status { get; set; }
}
