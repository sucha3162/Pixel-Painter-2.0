﻿using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;

namespace MyTestVueApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommentController : ControllerBase
    {
        private ILogger<CommentController> Logger { get; }
        private ICommentAccessService CommentAccessService { get; }
        private IOptions<ApplicationConfiguration> AppConfig { get; }
        public CommentController(ILogger<CommentController> logger, ICommentAccessService commentAccessService, IOptions<ApplicationConfiguration> appConfig) {
            Logger = logger;
            CommentAccessService = commentAccessService;
            AppConfig = appConfig;
        }

        [HttpGet]
        [Route("IsLoggedIn")]
        public IActionResult IsLoggedIn()
        {
            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                // You can add additional checks here if needed
                return Ok(!string.IsNullOrEmpty(userId));
            }
            return Ok(false);
        }

        [HttpGet]
        [Route("GetCommentsById")]
        public IEnumerable<Comment> GetCommentsById(int id)
        {
            return CommentAccessService.GetCommentsById(id);
        }

        [HttpGet]
        [Route("CheckCookietoUser")]
        public async Task<IActionResult> CheckCookietoUser(int commentId)
        {
            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                // You can add additional checks here if needed
                return Ok(userId == commentId.ToString());
            }
            return Ok(false);
        }

        [HttpGet]
        [Route("postComment")]
        public async Task<IActionResult> postComment(String comment, int ArtId)
        {
            if (Request.Cookies.TryGetValue("GoogleOAuth", out var userId))
            {
                await CommentAccessService.createComment(userId, comment, ArtId);

                return Ok();
            }
            return BadRequest("Placeholder");

        }
    }
}
