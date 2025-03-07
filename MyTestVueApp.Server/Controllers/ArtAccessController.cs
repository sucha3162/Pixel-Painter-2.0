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
            return ArtAccessService.GetAllArt().Where(art => art.isPublic).OrderByDescending(art => art.creationDate);
        }

        [HttpGet]
        [Route("GetArtists")]
        public IEnumerable<Artist> GetAllArt(int artId)
        {
            return ArtAccessService.GetArtists(artId);
        }

        [HttpGet]
        [Route("GetArtByLikes")]
        public IEnumerable<Art> GetArtByLikes(bool isAscending)
        {
            if (isAscending)
            {
                return ArtAccessService.GetAllArt().Where(art => art.isPublic).OrderBy(art => art.numLikes);
            }
            return ArtAccessService.GetAllArt().Where(art => art.isPublic).OrderByDescending(art => art.numLikes);
        }

        [HttpGet]
        [Route("GetArtByComments")]
        public IEnumerable<Art> GetArtByComments(bool isAscending)
        {
            if (isAscending)
            {
                return ArtAccessService.GetAllArt().Where(art => art.isPublic).OrderBy(art => art.numComments);
            }
            return ArtAccessService.GetAllArt().Where(art => art.isPublic).OrderByDescending(art => art.numComments);
        }

        [HttpGet]
        [Route("GetArtByDate")]
        public IEnumerable<Art> GetArtByDate(bool isAscending)
        {
            if (isAscending)
            {
                return ArtAccessService.GetAllArt().Where(art => art.isPublic).OrderBy(art => art.creationDate);
            }
            return ArtAccessService.GetAllArt().Where(art => art.isPublic).OrderByDescending(art => art.creationDate);
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

                    return Ok(result.Where(art => art.artistId.Contains(artist.id)).OrderByDescending(art => art.creationDate));
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

                if (art.isPublic)
                {
                    if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                    {
                        var artist = await LoginService.GetUserBySubId(userId);
                        art.currentUserIsOwner = (art.artistId.Contains(artist.id));
                    }
                    return Ok(art);
                }
                else
                {
                    if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
                    {
                        var artist = await LoginService.GetUserBySubId(userId);
                        art.currentUserIsOwner = (art.artistId.Contains(artist.id));
                        if (art.currentUserIsOwner)
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

                    if (art.id == 0) //New art
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

        [HttpGet]
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

                    if (!art.artistId.Contains(artist.id))
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
                        if(item.id == artist.id)
                        {
                            isAnArtist = true;
                        }
                    }
                    if(isAnArtist == false)
                    {
                        return Unauthorized("User is not authorized for this action");
                    }

                    await ArtAccessService.DeleteContributingArtist(artId,artist.id);

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
