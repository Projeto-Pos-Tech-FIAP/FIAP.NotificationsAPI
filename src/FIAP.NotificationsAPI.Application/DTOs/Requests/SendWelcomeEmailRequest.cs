using System.ComponentModel.DataAnnotations;

namespace FIAP.NotificationsAPI.Application.DTOs.Requests;

public class SendWelcomeEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;
}
