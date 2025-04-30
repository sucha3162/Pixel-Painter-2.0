using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageMagick;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;
using MyTestVueApp.Server.Models;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GIFCreationController : ControllerBase
    {
        private readonly ILogger<GIFCreationController> Logger;

        public GIFCreationController(ILogger<GIFCreationController> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Download a gif
        /// </summary>
        /// <param name="gifModel">Gif object to be downloaded</param>
        [HttpPost]
        [Route("CreateGif")]
        [ProducesResponseType(typeof(File), 200)] //Someone will have to update
        public async Task<IActionResult> CreateGif([FromBody] GIFModel gifModel)
        {
            if (gifModel.Frames == null || gifModel.Frames.Length == 0)
            {
                return BadRequest("There are no frames.");
            }

            using (var gif = new MagickImageCollection())
            {
                foreach (var frame in gifModel.Frames)
                {
                    byte[] imageBytes = Convert.FromBase64String(frame);
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        var gifFrame = new MagickImage(stream);
                        uint delayCS = (uint)(100 / gifModel.FPS); // Calculte the delay in centiseconds

                        gifFrame.AnimationDelay = delayCS; // Animation delay is in centiseconds
                        gif.Add(gifFrame);
                    }
                }

                gif.Optimize();

                using (var outputStream = new MemoryStream())
                {
                    gif.Write(outputStream, MagickFormat.Gif);
                    return File(outputStream.ToArray(), "image/gif", "output.gif");
                }
            }
        }
        [HttpPost]
        [Route("CreateGifCode")]
        public async Task<IActionResult> CreateGifCode([FromBody] GIFModel gifModel)
        {
            if (gifModel.Frames == null || gifModel.Frames.Length == 0)
            {
                return BadRequest("There are no frames.");
            }

            using (var gif = new MagickImageCollection())
            {
                foreach (var frame in gifModel.Frames)
                {
                    byte[] imageBytes = Convert.FromBase64String(frame);
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        var gifFrame = new MagickImage(stream);
                        uint delayCS = (uint)(100 / gifModel.FPS); // Calculte the delay in centiseconds

                        gifFrame.AnimationDelay = delayCS; // Animation delay is in centiseconds
                        gif.Add(gifFrame);
                    }
                }

                gif.Optimize();

                using (var outputStream = new MemoryStream())
                {
                    gif.Write(outputStream, MagickFormat.Gif);
                    return File(outputStream.ToArray(), "image/gif");
                }
            }
        }
    }
}
