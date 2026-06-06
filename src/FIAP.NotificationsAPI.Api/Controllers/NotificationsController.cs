using FIAP.NotificationsAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.NotificationsAPI.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ISendNotificationsServices _sendNotificationsServices;
        public NotificationsController(ISendNotificationsServices sendNotificationsServices)
        {
            _sendNotificationsServices = sendNotificationsServices;
        }

        [HttpPost("Welcome")]
        public async Task<IActionResult> SendWelcomeNotification()
        {
            return Ok();
        }
    }
}
