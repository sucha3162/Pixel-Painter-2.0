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

        /// <summary>
        /// Gets all comments of an artwork
        /// </summary>
        /// <param name="artId">Id of the art the comments belong to</param>
        /// <returns>A list of comments</returns>
        [HttpGet]
        [Route("GetCommentsByArtId")]
        [ProducesResponseType(typeof(List<Comment>), 200)]
        public async Task<IActionResult> GetCommentsByArtId([FromQuery] int artId)
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

        /// <summary>
        /// Update a comment in the database
        /// </summary>
        /// <param name="altComment">New comment object</param>
        [HttpPut]
        [Route("EditComment")]
        public async Task<IActionResult> EditComment([FromBody] Comment altComment)
        {
            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var comment = await CommentAccessService.GetCommentByCommentId(altComment.Id);
                    var subid = await LoginService.GetUserBySubId(userId);
                    if (comment.ArtistId == subid.Id)
                    {    // You can add additional checks here if needed
                        var rowsChanged = await CommentAccessService.EditComment(altComment.Id, altComment.Message);
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

        /// <summary>
        /// Removes a comment from the database
        /// </summary>
        /// <param name="commentId">Id of the comment to be removed</param>
        [HttpDelete]
        [Route("DeleteComment")]
        public async Task<IActionResult> DeleteComment([FromQuery] int commentId)
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

        /// <summary>
        /// Add a comment to the database
        /// </summary>
        /// <param name="comment">Comment to be added to the database</param>
        /// <returns>A comment object</returns>
        [HttpPost]
        [Route("CreateComment")]
        [ProducesResponseType(typeof(Comment), 200)]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
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
