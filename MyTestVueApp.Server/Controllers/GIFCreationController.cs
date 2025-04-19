using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageMagick;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;
using MyTestVueApp.Server.Models;

namespace MyTestVueApp.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GIFCreationController : ControllerBase
    {
        private ILogger<GIFCreationController> Logger { get; }

        public GIFCreationController(ILogger<GIFCreationController> logger)
        {
            Logger = logger;
        }

        [HttpPost]
        [Route("CreateGif")]
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
        public async Task<IActionResult> CreateGifCode([FromBody] List<string> frames)
        {
            if (frames == null || frames.Count == 0)
            {
                return BadRequest("There are no frames.");
            }

            using (var gif = new MagickImageCollection())
            {
                foreach (var frame in frames)
                {
                    byte[] imageBytes = Convert.FromBase64String(frame);
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        var gifFrame = new MagickImage(stream);
                        gifFrame.AnimationDelay = 100;
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
