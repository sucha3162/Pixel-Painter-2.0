using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace MyTestVueApp.Server.ServiceImplementations
{
    public class NotificationService : INotificationService
    {
        private readonly IOptions<ApplicationConfiguration> appConfig;
        private readonly ILogger<NotificationService> logger;
        private readonly IArtAccessService artService;
        private readonly ICommentAccessService commentService;
        private readonly ILikeService likeService;
        public NotificationService(IOptions<ApplicationConfiguration> AppConfig, ILogger<NotificationService> Logger, IArtAccessService ArtAccessService, ICommentAccessService CommentAccessService, ILikeService LikeService)
        {
            appConfig = AppConfig;
            logger = Logger;
            artService = ArtAccessService;
            commentService = CommentAccessService;
            likeService = LikeService;
        }

        public IEnumerable<Notification> GetNotificationsForArtist(int artistId)
        {
            var notifications = new List<Notification>();
            var artworks = artService.GetArtByArtist(artistId);
            foreach (Art artwork in artworks)
            {
                //Get notified on who commented on your art
                var comments = commentService.GetCommentsById(artwork.id);
                foreach(Comment comment in comments)
                {
                    if(comment.replyId == null) //don't want to get replies here, just original comments on art
                    {
                        var notification = new Notification
                        {
                            type = 0,
                            user = comment.commenterName,
                            viewed = comment.Viewed,
                            artName = artwork.title
                        };
                    }
                }
                //Get notified on who liked your art
                var likes = likeService.GetLikesByArtwork(artwork.id);
                foreach(Like like in likes) 
                {
                    var notification = new Notification
                    {
                        type = 1,
                        user = like.Artist,
                        viewed = like.Viewed,
                        artName = artwork.title
                    };
                    notifications.Add(notification);
                }
            }
            return notifications;
        }
    }
}
