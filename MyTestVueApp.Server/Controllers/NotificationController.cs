using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;
using System.Security.Authentication;

namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> logger;
        private readonly INotificationService notificationService;
        private readonly ICommentAccessService commentService;
        private readonly ILikeService likeService;

        public NotificationController(ILogger<NotificationController> Logger, INotificationService NotificationService, ICommentAccessService CommentService, ILikeService LikeService)
        {
            logger = Logger;
            notificationService = NotificationService;
            commentService = CommentService;
            likeService = LikeService;
        }

        [HttpGet]
        [Route("GetNotifications")]
        public IActionResult GetNotifications([FromQuery] string userId){
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

        [HttpPut]
        [Route("MarkCommentViewed")]
        public async Task<IActionResult> MarkCommentViewed([FromQuery] string viewedComment)
        {
            try
            {
                int commentId;
                if(int.TryParse(viewedComment, out commentId))
                {
                    Comment comment = await commentService.GetCommentByCommentId(commentId);
                    if (comment != null)
                    {
                        if(await notificationService.MarkComment(commentId))
                        {
                            return Ok();
                        } else
                        {
                            throw new Exception("An unknown Error has Occured");
                        }
                        
                    } else
                    {
                        throw new ArgumentException("Comment with id: " + viewedComment + " was not found");
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

        [HttpPut]
        [Route("MarkLikeViewed")]
        public async Task<IActionResult> MarkLikeViewed([FromQuery] string viewedArtId, [FromQuery] string viewedArtistId)
        {
            try
            {
                int artId, artistId;
                if (int.TryParse(viewedArtId, out artId) && int.TryParse(viewedArtistId, out artistId))
                {
                    Like like = await likeService.GetLikeByIds(artId, artistId);
                    if (like != null)
                    {
                        if (await notificationService.MarkLike(artId, artistId))
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
                        throw new ArgumentException("Like with art id: " + viewedArtId + " and artist id: "+ viewedArtistId+ " was not found");
                    }
                }
                else
                {
                    throw new HttpRequestException("CommentId must be an int");
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
