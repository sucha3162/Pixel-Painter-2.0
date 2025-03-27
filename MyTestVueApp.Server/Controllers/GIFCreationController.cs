using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageMagick;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;

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
        public async Task<IActionResult> CreateGif([FromBody] List<string> frames)
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
                    return File(outputStream.ToArray(), "image/gif", "output.gif");
                }
            }
        }
    }
}
