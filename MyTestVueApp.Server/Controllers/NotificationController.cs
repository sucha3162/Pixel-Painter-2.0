using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.Models;
using MyTestVueApp.Server.ServiceImplementations;
using System.Security.Authentication;

namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> Logger;
        private readonly INotificationService NotificationService;
        private readonly ICommentAccessService CommentService;
        private readonly ILikeService LikeService;

        public NotificationController(ILogger<NotificationController> logger, INotificationService notificationService, ICommentAccessService commentService, ILikeService likeService)
        {
            Logger = logger;
            NotificationService = notificationService;
            CommentService = commentService;
            LikeService = likeService;
        }
        /// <summary>
        /// Gets all notifications for a user
        /// </summary>
        /// <param name="userId">Id of the user to get notifications for</param>
        /// <returns>A list of notifications</returns>
        [HttpGet]
        [Route("GetNotifications")]
        [ProducesResponseType(typeof(List<Notification>), 200)]
        public async Task<IActionResult> GetNotifications([FromQuery] string userId){
            try
            {
                int uid;
                if (int.TryParse(userId, out uid))
                {
                    var notifications = await NotificationService.GetNotificationsForArtist(uid);
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
        /// <summary>
        /// Marks a comment as viewed in the database
        /// </summary>
        /// <param name="commentId">Comment to mark</param>
        [HttpPost]
        [Route("MarkCommentViewed")]
        public async Task<IActionResult> MarkCommentViewed([FromBody] int commentId)
        {
            try
            {
                Comment comment = await CommentService.GetCommentByCommentId(commentId);
                if (comment != null)
                {
                    if(await NotificationService.MarkComment(comment.Id))
                    {
                        return Ok();
                    } else
                    {
                        throw new Exception("An unknown Error has Occured");
                    }
                    
                }
                else
                {
                    throw new HttpRequestException("CommentId must be an int");
                }
            } catch (HttpRequestException ex)
            {
                return BadRequest(ex.Message);
            } catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Marks a like viewed in the database
        /// </summary>
        /// <param name="likeModel">Id of the art, and id of the artist</param>
        [HttpPost]
        [Route("MarkLikeViewed")]
        public async Task<IActionResult> MarkLikeViewed([FromBody] LikesModel likeModel)
        {
            try
            {
                Like like = await LikeService.GetLikeByIds(likeModel.ArtId, likeModel.ArtistId);
                if (like != null)
                {
                    if (await NotificationService.MarkLike(like.ArtId, like.ArtistId))
                    {
                        return Ok();
                    }
                    else
                    {
                        throw new Exception("An unknown Error has Occured");
                    }
                }
                else
                {
                    throw new ArgumentException("Like with art id: " + likeModel.ArtId + " and artist id: "+ likeModel.ArtistId+ " was not found");
                }
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
