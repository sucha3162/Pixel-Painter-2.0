using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;


namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommentController : ControllerBase
    {
        private ILogger<CommentController> Logger { get; }
        private ICommentAccessService CommentAccessService { get; }
        private IOptions<ApplicationConfiguration> AppConfig { get; }
        private ILoginService LoginService { get; }
         
        public CommentController(ILogger<CommentController> logger, ICommentAccessService commentAccessService, IOptions<ApplicationConfiguration> appConfig, ILoginService loginService)
        {
            Logger = logger;
            CommentAccessService = commentAccessService;
            AppConfig = appConfig;
            LoginService = loginService;
        }

        [HttpGet]
        [Route("GetCommentsByArtId")]
        public async Task<IEnumerable<Comment>> GetCommentsByArtId(int artId)
        {
            var comments = CommentAccessService.GetCommentsById(artId);

            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                var artist = await LoginService.GetUserBySubId(userId);
                if(artist == null)
                {
                    return comments;
                }
                foreach (var comment in comments)
                {
                    comment.currentUserIsOwner = comment.artistId == artist.id;
                }
            }

            return comments;
        }

        [HttpGet]
        [Route("EditComment")]
        public async Task<IActionResult> EditComment(int commentId, String newMessage)
        {

            // If the user is logged in
            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                var comment = CommentAccessService.GetCommentByCommentId(commentId);
                var subid = await LoginService.GetUserBySubId(userId);
                if(comment.artistId == subid.id)
                {    // You can add additional checks here if needed
                    var rowsChanged = await CommentAccessService.EditComment(commentId, newMessage);
                    if (rowsChanged > 0) // If the comment has been sucessfuly edited
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest("Failed to edit comment. User may have already editied this comment.");
                    }
                }
                else
                {
                    return Unauthorized("User does not have permissions for this action");
                }
            }
            else
            {
                return BadRequest("User is not logged in");
            }
            

        }


        [HttpGet]
        [Route("DeleteComment")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {

            // If the user is logged in
            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                var comment = CommentAccessService.GetCommentByCommentId(commentId);
                var artist = await LoginService.GetUserBySubId(userId);
                var subid = await LoginService.GetUserBySubId(userId);
                if (comment.artistId == subid.id || artist.isAdmin)
                {
                    // You can add additional checks here if needed
                    var rowsChanged = await CommentAccessService.DeleteComment(commentId);
                    if (rowsChanged > 0) // If the comment has been deleted
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest("Failed to delete Comment");
                    }
                }
                else
                {
                    return Unauthorized("User does not have permissions for this action");
                }

            }
            else
            {
                return BadRequest("User is not logged in");
            }

        }

        [HttpPost]
        [Route("CreateComment")]
        public async Task<IActionResult> CreateComment(Comment comment)
        {
            try
            {
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    if (artist != null)
                    {
                        var result = await CommentAccessService.CreateComment(artist, comment);
                        return Ok(result);
                    }
                }
                return BadRequest("User not logged in");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

        }
    }
}
