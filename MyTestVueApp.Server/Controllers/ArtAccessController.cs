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
        private readonly ILogger<ArtAccessController> Logger; 
        private readonly IArtAccessService ArtAccessService;
        private readonly ILoginService LoginService;

        public ArtAccessController(ILogger<ArtAccessController> logger, IArtAccessService artAccessService, ILoginService loginService)
        {
            Logger = logger;
            ArtAccessService = artAccessService;
            LoginService = loginService;
        }

        [HttpGet]
        [Route("GetAllArt")]
        public async Task<IActionResult> GetAllArt()
        {
            try
            {
                var art = await ArtAccessService.GetAllArt();
                var artList = art.Where(art => art.IsPublic).OrderByDescending(art => art.CreationDate);
                return Ok(artList);
            } 
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetLikedArt")]
        public async Task<IActionResult> GetLikedArt(int artistId)
        {
            try
            {
                var art = await ArtAccessService.GetLikedArt(artistId);
                var artList = art.OrderByDescending(art => art.CreationDate);
                return Ok(artList);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAllArtByUserID")]
        public async Task<IActionResult> GetAllArtByUserID(int id)
        {
            try
            {
                var artistArt = await ArtAccessService.GetArtByArtist(id);
                var artistArtList = artistArt.Where(art => art.IsPublic).OrderByDescending(art => art.CreationDate);
                return Ok(artistArtList);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        //Remove
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

                    var result = await ArtAccessService.GetAllArt();
                    return Ok(result.Where(art => art.ArtistId.Contains(artist.Id)).OrderByDescending(art => art.CreationDate));
                   
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
        
        /*[HttpGet]
        [Route("GetArtists")]
        public async Task<IActionResult> GetAllArtists(int artId)
        {
            return Ok(await ArtAccessService.GetArtistsByArtId(artId));
        }*/

        [HttpPost]
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


        [HttpDelete]
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

                    if (!(art.ArtistId.Contains(artist.Id)) && !artist.IsAdmin)
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

        [HttpDelete]
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
                    var artists = await ArtAccessService.GetArtistsByArtId(artId);

                    foreach (var item in artists)
                    {
                        if (item.Id == artist.Id || artist.IsAdmin)
                        {
                            isAnArtist = true;
                        }
                    }
                    if ((!isAnArtist) && (!artist.IsAdmin))
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
            } catch (AuthenticationException ex)
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
