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
using System.Security.Authentication;


namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ILogger<CommentController> Logger;
        private readonly ICommentAccessService CommentAccessService;
        private readonly IOptions<ApplicationConfiguration> AppConfig;
        private readonly ILoginService LoginService;
         
        public CommentController(ILogger<CommentController> logger, ICommentAccessService commentAccessService, IOptions<ApplicationConfiguration> appConfig, ILoginService loginService)
        {
            Logger = logger;
            CommentAccessService = commentAccessService;
            AppConfig = appConfig;
            LoginService = loginService;
        }

        [HttpGet]
        [Route("GetCommentsByArtId")]
        public async Task<IActionResult> GetCommentsByArtId(int artId)
        {
            try
            {
                var comments = await CommentAccessService.GetCommentsByArtId(artId);

                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    if (artist == null)
                    {
                        return Ok(comments);
                    }
                    foreach (var comment in comments)
                    {
                        comment.CurrentUserIsOwner = comment.ArtistId == artist.Id;
                    }
                }

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost]
        [Route("EditComment")]
        public async Task<IActionResult> EditComment(int commentId, String newMessage)
        {
            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var comment = await CommentAccessService.GetCommentByCommentId(commentId);
                    var subid = await LoginService.GetUserBySubId(userId);
                    if (comment.ArtistId == subid.Id)
                    {    // You can add additional checks here if needed
                        var rowsChanged = await CommentAccessService.EditComment(commentId, newMessage);
                        if (rowsChanged > 0) // If the comment has been sucessfuly edited
                        {
                            return Ok();
                        }
                        else
                        {
                            throw new ArgumentException("Failed to edit comment.");
                        }
                    }
                    else
                    {
                        throw new AuthenticationException("User does not have permissions for this action.");
                    }
                }
                else
                {
                    throw new AuthenticationException("User is not logged in.");
                }
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteComment")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var comment = await CommentAccessService.GetCommentByCommentId(commentId);
                    var artist = await LoginService.GetUserBySubId(userId);
                    var subid = await LoginService.GetUserBySubId(userId);
                    if (comment.ArtistId == subid.Id || artist.IsAdmin)
                    {
                        // You can add additional checks here if needed
                        var rowsChanged = await CommentAccessService.DeleteComment(commentId);
                        if (rowsChanged > 0) // If the comment has been deleted
                        {
                            return Ok();
                        }
                        else
                        {
                            throw new ArgumentException("Failed to edit comment.");
                        }
                    }
                    else
                    {
                        throw new AuthenticationException("User does not have permissions for this action");
                    }

                }
                else
                {
                    throw new ArgumentException("User is not logged in");
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch(Exception ex)
            {
                return Problem(ex.Message);
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
                throw new AuthenticationException("User not logged in");
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
