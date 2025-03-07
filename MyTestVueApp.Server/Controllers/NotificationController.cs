using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;
using System.Security.Authentication;

namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("notification")]
    public class NotificationController : ControllerBase
    {
        //need Service
        private readonly ILogger<NotificationController> logger;
        private readonly INotificationService notificationService;

        public NotificationController(ILogger<NotificationController> Logger, INotificationService NotificationService)
        {
            logger = Logger;
            notificationService = NotificationService;
        }

        [HttpGet]
        [Route("GetNotifications")]
        public async Task<IActionResult> GetNotifications(string userId){
            try
            {
                int uid;
                if (int.TryParse(userId, out uid))
                {
                    var notifications = notificationService.GetNotificationsForArtist(uid);
                    return Ok(notifications);
                }
                else
                {
                    throw new HttpRequestException("User Id is an int");
                }
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
