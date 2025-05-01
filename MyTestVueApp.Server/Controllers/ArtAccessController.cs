using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        /// <summary>
        /// Gets All Art
        /// </summary>
        [HttpGet]
        [Route("GetAllArt")]
        [ProducesResponseType(typeof(List<Art>), 200)]
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

        /// <summary>
        /// Obtains all art that a user has liked
        /// </summary>
        /// <param name="artistId">Id of the artist being checked</param>
        /// <returns>A list of art objects</returns>
        [HttpGet]
        [Route("GetLikedArt")]
        [ProducesResponseType(typeof(List<Art>), 200)]
        public async Task<IActionResult> GetLikedArt([FromQuery] int artistId)
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
        /// <summary>
        /// Gets all art made by a user
        /// </summary>
        /// <param name="id">Id of user</param>
        /// <returns>A list of art objects</returns>
        [HttpGet]
        [Route("GetAllArtByUserID")]
        [ProducesResponseType(typeof(List<Art>), 200)]
        public async Task<IActionResult> GetAllArtByUserID([FromQuery] int id)
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

        /// <summary>
        /// Gets the current user's art from the database
        /// </summary>
        /// <returns>A list of Art Objects</returns>
        [HttpGet]
        [Route("GetCurrentUsersArt")]
        [ProducesResponseType(typeof(List<Art>), 200)]
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
        /// <summary>
        /// Gets an artwork from the database
        /// </summary>
        /// <param name="id">Id of the artwork</param>
        /// <returns>An art object</returns>
        [HttpGet]
        [Route("GetArtById")]
        [ProducesResponseType(typeof(Art), 200)]
        public async Task<IActionResult> GetArtById([FromQuery] int id)
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
        /// Gets a gif from the database
        /// </summary>
        /// <param name="id">Id of the gif to grab</param>
        [HttpGet]
        [Route("GetGifById")]
        [ProducesResponseType(typeof(Art[]), 200)]
        public async Task<IActionResult> GetGifById(int id)
        {
            try
            {
                var gif = await ArtAccessService.GetGif(id);
                return Ok(gif);
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
        /// Add a Gif to the database
        /// </summary>
        /// <param name="art">The Gif being uploaded</param>
        /// <param name="fps">The frames per second of the Gif</param>
        [HttpPut]
        [Route("SaveGif")]
        public async Task<IActionResult> SaveGif([FromBody] Art[] art, int fps)
        {
            try
            {
                if (Request.Cookies.TryGetValue("GoogleOAuth", out var userSubId))
                {
                    var artist = await LoginService.GetUserBySubId(userSubId);

                    if (artist == null)
                    {
                        return BadRequest("User not logged in");
                    }

                    if (art[0].Id == 0) //New Gif
                    {
                        var result = await ArtAccessService.SaveGif(artist, art);
                        return Ok(result);
                    }
                    else //Update Gif
                    {
                        var result = await ArtAccessService.UpdateGif(art, fps);
                        if (result == null)
                        {
                            return BadRequest("Could not update this gif"); //need to update fps as well
                        }
                        return Ok(result);
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
        }

        /// <summary>
        /// Add/Update an art in the database
        /// </summary>
        /// <param name="art">New Art object</param>
        /// <returns></returns>
        [HttpPut]
        [Route("SaveArt")]
        [ProducesResponseType(typeof(Art), 200)]
        public async Task<IActionResult> SaveArt([FromBody, BindRequired]Art art)
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
                            if(artistId == artist.Id)
                            {
                                continue;
                            }
                            ArtAccessService.AddContributingArtist(art.Id, artistId);
                        }
                        return Ok(result);
                    }
                    else //Update art
                    {
                        var result = await ArtAccessService.UpdateArt(art);
                        if (result == null)
                        {
                            return BadRequest("Could not update this art");
                        }
                        return Ok(result);
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

        /// <summary>
        /// Removes an artwork from the database
        /// </summary>
        /// <param name="artId">Id of the art to be removed</param>
        [HttpDelete]
        [Route("DeleteArt")]
        public async Task<IActionResult> DeleteArt([FromQuery] int artId)
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

        /// <summary>
        /// Removes an artist from an artworks contributers list
        /// </summary>
        /// <param name="artId">Id of the artwork to remove the user from</param>
        [HttpDelete]
        [Route("DeleteContributingArtist")]
        public async Task<IActionResult> DeleteContrbutingArtist([FromQuery] int artId)
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
