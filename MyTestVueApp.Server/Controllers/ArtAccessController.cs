using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    public class ArtAccessController : ControllerBase
    {
        private ILogger<ArtAccessController> Logger { get; }
        private IArtAccessService ArtAccessService { get; }
        private ILoginService LoginService { get; }

        public ArtAccessController(ILogger<ArtAccessController> logger, IArtAccessService artAccessService, ILoginService loginService)
        {
            Logger = logger;
            ArtAccessService = artAccessService;
            LoginService = loginService;
        }

        [HttpGet]
        [Route("GetAllArt")]
        public async Task<IEnumerable<Art>> GetAllArt()
        {
            return (await ArtAccessService.GetAllArt()).Where(art => art.IsPublic).OrderByDescending(art => art.CreationDate);
        }

        [HttpGet]
        [Route("GetArtists")]
        public async Task<IEnumerable<Artist>> GetAllArtists(int artId)
        {
            return await ArtAccessService.GetArtists(artId);
        }

        [HttpGet]
        [Route("GetCurrentUsersArt")]
        public async Task<IActionResult> GetCurrentUsersArt()
        {
            try
            {
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userSubId))
                {
                    var artist = await LoginService.GetUserBySubId(userSubId);

                    if (artist == null)
                    {
                        throw new AuthenticationException("User does not have an account.");
                    }

                    var result = await ArtAccessService.GetArtByArtist(artist.Id);

                    return Ok(result.OrderByDescending(art => art.CreationDate));
                }
                else
                {
                    throw new AuthenticationException("User is not logged in.");
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
        [Route("GetArtById")]
        public async Task<IActionResult> GetArtById(int id)
        {
            try
            {
                var art = await ArtAccessService.GetArtById(id);

                if (art == null)
                {
                    throw new ArgumentException("Art with id: " + id + " can not be found");
                }

                if (art.IsPublic)
                {
                    if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                    {
                        var artist = await LoginService.GetUserBySubId(userId);
                        if (artist != null)
                        {
                            art.CurrentUserIsOwner = art.ArtistId.Contains(artist.Id);
                        }
                    }
                    return Ok(art);
                }
                else
                {
                    if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                    {
                        var artist = await LoginService.GetUserBySubId(userId);
                        art.CurrentUserIsOwner = art.ArtistId.Contains(artist.Id);
                        if (art.CurrentUserIsOwner)
                        {
                            return Ok(art);
                        }
                        else
                        {
                            throw new AuthenticationException("User does not have permission for this action");
                        }
                    }
                    else
                    {
                        throw new AuthenticationException("User is not logged in");
                    }
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
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
        /// 
        /// </summary>
        /// <param name="art"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SaveArt")]
        public async Task<IActionResult> SaveArt(Art art)
        {
            try
            {
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userSubId))
                {
                    var artist = await LoginService.GetUserBySubId(userSubId);

                    if (artist == null)
                    {
                        throw new AuthenticationException("User does not have an account.");
                    }

                    if (art.Id == 0) //New art
                    {
                        var result = await ArtAccessService.SaveNewArt(artist, art);
                        foreach (int artistId in art.ArtistId)
                        {
                            ArtAccessService.AddContributingArtist(art.Id, artistId);
                        }
                        return Ok(result.Id);
                    }
                    else //Update art
                    {
                        var result = await ArtAccessService.UpdateArt(artist, art);
                        if (result == null)
                        {
                            return BadRequest("Could not update this art");
                        }
                        return Ok(result.Id);
                    }
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

        /*[HttpPost]
        [Route("SaveArtCollab")]
        public async Task<IActionResult> SaveArtCollab(Art art)
        {
            try
            {
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userSubId))
                {
                    var artist = await LoginService.GetUserBySubId(userSubId);

                    if (artist == null)
                    {
                        throw new AuthenticationException("User does not have permission for this action");
                    }

                    if (art.Id == 0) //New art
                    {
                        var result = await ArtAccessService.SaveNewArtMulti(art);
                        // If there are attatched contributing artists
                        foreach (int artistId in art.ArtistId)
                        {
                            ArtAccessService.AddContributingArtist(art.Id, artistId);
                        }
                        return Ok(result);
                    }
                    else //Update art
                    {
                        return BadRequest("Could not update this art");
                    }
                }
                else
                {
                    return BadRequest("User not logged in");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }*/

        /*[HttpGet]
        [Route("IsMyArt")]
        public async Task<bool> IsMyArt(int id)
        {
            var art = ArtAccessService.GetArtById(id);
            bool ismine = false;

            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                var artist = await LoginService.GetUserBySubId(userId);

                ismine = (art.artistId.Contains(artist.id));
            }
            return ismine;
        }*/


        [HttpGet]
        [Route("DeleteArt")]
        public async Task<IActionResult> DeleteArt(int artId)
        {
            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var artist = await LoginService.GetUserBySubId(userId);
                    var art = await ArtAccessService.GetArtById(artId);

                    if (!art.ArtistId.Contains(artist.Id))
                    {
                        throw new AuthenticationException("User does not have permissions for this artwork.");
                    }

                    ArtAccessService.DeleteArt(artId);

                    return Ok();

                }
                else
                {
                    throw new AuthenticationException("User is not logged in.");
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
        [Route("DeleteContributingArtist")]
        public async Task<IActionResult> DeleteContrbutingArtist(int artId)
        {

            try
            {
                // If the user is logged in
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                {
                    var isAnArtist = false;
                    var artist = await LoginService.GetUserBySubId(userId);
                    var artists = await ArtAccessService.GetArtists(artId);
                    foreach (var item in artists)
                    {
                        if(item.Id == artist.Id)
                        {
                            isAnArtist = true;
                        }
                    }
                    if(isAnArtist == false) //// Check if user is an admin
                    {
                        throw new AuthenticationException("User does not have permission to remove user.");
                    }

                    ArtAccessService.DeleteContributingArtist(artId, artist.Id);

                    return Ok();

                }
                else
                {
                    throw new AuthenticationException("User is not logged in.");
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

        }
    }
}
