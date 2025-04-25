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
        public IEnumerable<Art> GetAllArt()
        {
            return ArtAccessService.GetAllArt().Where(art => art.IsPublic).OrderByDescending(art => art.CreationDate);
        }

        [HttpGet]
        [Route("GetAllArtByUserID")]
        public IEnumerable<Art> GetAllArtByUserID(int id)
        {
            return ArtAccessService.GetArtByArtist(id).Where(art => art.IsPublic).OrderByDescending(art => art.CreationDate);
        }

        [HttpGet]
        [Route("GetLikedArt")]

        public async Task<IEnumerable<Art>> GetLikedArt(int artistId)
        {
            return await ArtAccessService.GetLikedArt(artistId);
        }

        [HttpGet]
        [Route("GetArtByLikes")]
        public IEnumerable<Art> GetArtByLikes(bool isAscending)
        {
            if (isAscending)
            {
                return ArtAccessService.GetAllArt().Where(art => art.IsPublic).OrderBy(art => art.NumLikes);
            }
            return ArtAccessService.GetAllArt().Where(art => art.IsPublic).OrderByDescending(art => art.NumLikes);
        }

        [HttpGet]
        [Route("GetArtByComments")]
        public IEnumerable<Art> GetArtByComments(bool isAscending)
        {
            if (isAscending)
            {
                return ArtAccessService.GetAllArt().Where(art => art.IsPublic).OrderBy(art => art.NumComments);
            }
            return ArtAccessService.GetAllArt().Where(art => art.IsPublic).OrderByDescending(art => art.NumComments);
        }

        [HttpGet]
        [Route("GetArtByDate")]
        public IEnumerable<Art> GetArtByDate(bool isAscending)
        {
            if (isAscending)
            {
                return ArtAccessService.GetAllArt().Where(art => art.IsPublic).OrderBy(art => art.CreationDate);
            }
            return ArtAccessService.GetAllArt().Where(art => art.IsPublic).OrderByDescending(art => art.NumComments);
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
                        return BadRequest("User not logged in");
                    }

                    var result = ArtAccessService.GetAllArt();
                    return Ok(result.Where(art => art.ArtistId.Contains(artist.id)).OrderByDescending(art => art.CreationDate));
                   
                }
                else
                {
                    return BadRequest("User not logged in");
                }
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
                var art = ArtAccessService.GetArtById(id);

                if (art == null)
                {
                    return BadRequest("Art not found");
                }

                if (art.IsPublic)
                {
                    if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                    {
                        var artist = await LoginService.GetUserBySubId(userId);
                        if (artist != null)
                        {
                            art.CurrentUserIsOwner = (art.ArtistId.Contains(artist.id));
                        }
                    }
                    return Ok(art);
                }
                else
                {
                    if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                    {
                        var artist = await LoginService.GetUserBySubId(userId);
                        art.CurrentUserIsOwner = (art.ArtistId.Contains(artist.id));
                        if (art.CurrentUserIsOwner)
                        {

                            return Ok(art);
                        }
                        else
                        {
                            return Unauthorized("User does not have permission for this action");
                        }
                    }
                    else
                    {
                        return Unauthorized("User does not have permission for this action");
                    }
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        [HttpGet]
        [Route("GetGif")]
        public async Task<IActionResult> GetGif(int id)
        {
            try
            {
                var gif = await ArtAccessService.GetGif(id);
                return Ok(gif);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetArtists")]
        public IEnumerable<Artist> GetAllArtists(int artId)
        {
            return ArtAccessService.GetArtists(artId);
        }

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
                        return BadRequest("User not logged in");
                    }

                    if (art.Id == 0) //New art
                    {
                        var result = await ArtAccessService.SaveNewArt(artist, art);
                        return Ok(result);
                    }
                    else //Update art
                    {
                        var result = await ArtAccessService.UpdateArt(artist, art);
                        if (result == null)
                        {
                            return BadRequest("Could not update this art");
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
                        var result = await ArtAccessService.UpdateGif(art,fps);
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

        [HttpPost]
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
                        return BadRequest("User not logged in");
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
        }

        [HttpGet]
        [Route("IsMyArt")]
        public async Task<bool> IsMyArt(int id)
        {
            var art = ArtAccessService.GetArtById(id);
            bool ismine = false;

            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                var artist = await LoginService.GetUserBySubId(userId);

                ismine = (art.ArtistId.Contains(artist.id));
            }
            return ismine;
        }


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
                    var art = ArtAccessService.GetArtById(artId);

                    if (!(art.ArtistId.Contains(artist.id)) && !artist.isAdmin)
                    {
                        return Unauthorized("User is not authorized for this action");
                    }

                    await ArtAccessService.DeleteArt(artId);

                    return Ok();

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
                    var artists = ArtAccessService.GetArtists(artId);

                    foreach (var item in artists)
                    {
                        if (item.id == artist.id || artist.isAdmin)
                        {
                            isAnArtist = true;
                        }
                    }
                    if ((!isAnArtist) && (!artist.isAdmin))
                    {
                        return Unauthorized("User is not authorized for this action");
                    }

                    await ArtAccessService.DeleteContributingArtist(artId, artist.id);

                    return Ok();

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
    }
}
