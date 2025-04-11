using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using MyTestVueApp.Server.ServiceImplementations;
using MyTestVueApp.Server.Entities;
using System.Security.Authentication;

namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IOptions<ApplicationConfiguration> AppConfig;
        private readonly ILogger<ArtAccessController> Logger;
        private readonly ILoginService LoginService;

        public LoginController(IOptions<ApplicationConfiguration> appConfig, ILogger<ArtAccessController> logger, ILoginService loginService)
        {
            AppConfig = appConfig;
            Logger = logger;
            LoginService = loginService;
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            var returnUrl = AppConfig.Value.HomeUrl + "login/LoginRedirect";
            var url = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={AppConfig.Value.ClientId}&redirect_uri={returnUrl}&scope=email profile&response_type=code&prompt=consent";

            return Redirect(url);
        }

        [HttpGet]
        [Route("LoginRedirect")]
        public async Task<IActionResult> RedirectLogin(string code, string scope, string authuser, string prompt)
        {
            var userInfo = await LoginService.GetUserId(code);

            // Add Id to cookies
            Response.Cookies.Append("GoogleOAuth", userInfo.Id, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            await LoginService.SignupActions(userInfo.Id, userInfo.Email);

            return Redirect(AppConfig.Value.HomeUrl);
        }

        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("GoogleOAuth");
            return Ok();
        }

        [HttpGet]
        [Route("IsLoggedIn")]
        public async Task<IActionResult> IsLoggedIn()
        {   
            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                var artist = await LoginService.GetUserBySubId(userId);
                return Ok(artist != null);
            }
            return Ok(false);
        }

        [HttpGet]
        [Route("GetAllArtists")]

        public async Task<IActionResult> GetAllArtists()
        {
            try
            {
                var artist = await LoginService.GetAllArtists();
                return Ok(artist);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    return Ok(artist);
                }
                throw new AuthenticationException("User is not logged in.");
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

        [HttpGet]
        [Route("GetArtistByName")]
        public async Task<Artist> GetArtistByName(string name)
        {
            return await LoginService.GetArtistByName(name);
        }

        [HttpGet]
        [Route("GetIsAdmin")]
        public async Task<IActionResult> GetIsAdmin()
        {
            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    if(artist == null) { return Ok(false); }
                    if (artist.IsAdmin)
                    {
                        return Ok(true);
                    }
                    else { return Ok(false); }

                }
                else
                {
                    return BadRequest("User is not logged in");
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        [Route("UpdateUsername")]
        public async Task<IActionResult> UpdateUsername(string newUsername)
        {
            try
            {
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var subId))
                {
                    var success = await LoginService.UpdateUsername(newUsername, subId);
                    return Ok(success);
                }
                else
                {
                    throw new AuthenticationException("User is not logged in");
                }
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

        [HttpGet]
        [Route("DeleteArtist")]
        public async Task<IActionResult> DeleteArtist(int id)
        {
            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    if(artist.Id == id)
                    {
                        LoginService.DeleteArtist(artist.Id);
                        Response.Cookies.Delete("GoogleOAuth");
                        return Ok();
                    }
                    else if (artist.IsAdmin)
                    {
                        LoginService.DeleteArtist(artist.Id);
                        return Ok();
                    }
                    else {
                        throw new InvalidCredentialException("User is not allowed to preform this action");
                    }
                }
                else
                {
                    throw new AuthenticationException("User is not logged in");
                }
            }
            catch (InvalidCredentialException ex)
            {
                return Forbid(ex.Message);
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
