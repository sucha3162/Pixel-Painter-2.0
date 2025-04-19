using Microsoft.AspNetCore.Mvc;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SocketController : ControllerBase
    {
        private readonly ILogger<SocketController> Logger;
        private readonly IConnectionManager ConnectionManager;
        
        public SocketController(IConnectionManager connectionManager, ILogger<SocketController> logger)
        {
            ConnectionManager = connectionManager;
            Logger = logger;
        }

        [HttpGet]
        [Route("GetGroups")]
        public IActionResult GetGroups()
        {
            try
            {
                return Ok(ConnectionManager.GetGroupAdverts());
            } catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
