using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;
using System.Security.Authentication;


namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly IOptions<ApplicationConfiguration> AppConfig;
        private readonly ILogger<LikeController> Logger;
        private readonly ILikeService LikeService;
        private readonly IArtAccessService ArtService;
        private readonly ILoginService LoginService;

        public LikeController(IOptions<ApplicationConfiguration> appConfig, ILogger<LikeController> logger, ILikeService likeService, ILoginService loginService, IArtAccessService artService)
        {
            AppConfig = appConfig;
            Logger = logger;
            ArtService = artService;
            LikeService = likeService;
            LoginService = loginService;
        }

        [HttpGet]
        [Route("InsertLike")]
        public async Task<IActionResult> InsertLike(int artId)
        {
            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    if (artist != null)
                    {
                        // You can add additional checks here if needed
                        var rowsChanged = await LikeService.InsertLike(artId, artist);
                        if (rowsChanged > 0) // If the like has sucessfully been inserted
                        {
                            return Ok();
                        }
                        else
                        {
                            throw new ArgumentException("Failed to insert like. User may have already liked this post.");
                        }
                    }
                    else
                    {
                        throw new AuthenticationException("User does not have an account.");
                    }
                }
                else
                {
                    throw new AuthenticationException("User is not logged in!");
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
        [Route("RemoveLike")]
        public async Task<IActionResult> RemoveLike(int artId)
        {
            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    if (artist != null)
                    {
                        // You can add additional checks here if needed
                        var rowsChanged = await LikeService.RemoveLike(artId, artist);
                        if (rowsChanged > 0) // If the like has sucessfully been removed
                        {
                            return Ok();
                        }
                        else
                        {
                            throw new ArgumentException("Did not remove target like. It may not have existed.");
                        }
                    }
                    else
                    {
                        throw new AuthenticationException("User does not have an account");
                    }
                }
                else
                {
                    throw new AuthenticationException("User is not logged in!");
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

        [HttpPost]
        [Route("IsLiked")]
        public async Task<IActionResult> IsLiked(int artId)
        {
            try
            {
                var art = ArtService.GetArtById(artId);
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    if(await art == null)
                    {
                        throw new ArgumentOutOfRangeException("Art was not found.");
                    }
                    if (artist != null)
                    {
                        var liked = await LikeService.IsLiked(artId, artist);
                        return Ok(liked);
                    }
                    else
                    {
                        throw new AuthenticationException("User does not have an account");
                    }
                }
                else
                {
                    throw new AuthenticationException("User is not logged in!");
                }
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch(ArgumentOutOfRangeException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
