using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Identity.Client;
using System.ComponentModel.Design;

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

        public async Task<IEnumerable<Notification>> GetNotificationsForArtist(int artistId)
        {
            DateTime thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var notifications = new List<Notification>();
            var artworks = await artService.GetArtByArtist(artistId);
            foreach (Art artwork in artworks)
            {
                //Get notified on who commented on your art
                var comments = await commentService.GetCommentsByArtId(artwork.Id);
                foreach(Comment comment in comments)
                {
                    if(comment.ArtistId == artistId || comment.CreationDate < thirtyDaysAgo) //Make sure it is not the user, or over 30 days old
                    {
                        continue;
                    }
                    else if(comment.ReplyId == null || comment.ReplyId == 0) //don't want to get replies here, just original comments on art
                    {
                        var notification = new Notification
                        {
                            CommentId = comment.Id,
                            ArtId = -1,
                            ArtistId = -1,
                            Type = 0,
                            User = comment.CommenterName,
                            Viewed = comment.Viewed,
                            ArtName = artwork.Title
                        };
                        notifications.Add(notification);
                    }
                }
                //Get notified on who liked your art
                var likes = await likeService.GetLikesByArtwork(artwork.Id);
                foreach(Like like in likes) 
                {
                    if (like.ArtistId == artistId || like.LikedOn < thirtyDaysAgo) //Make sure it is not the user, or over 30 days old
                    {
                        continue;
                    }
                    var notification = new Notification
                    {
                        CommentId = -1,
                        ArtId = like.ArtId,
                        ArtistId = like.ArtistId,
                        Type = 1,
                        User = like.Artist,
                        Viewed = like.Viewed,
                        ArtName = artwork.Title
                    };
                    notifications.Add(notification);
                }
            }
            var userComments = await commentService.GetCommentByUserId(artistId);
            foreach(Comment comment in userComments)
            {
                var replies = await commentService.GetReplyByCommentId(comment.Id);
                foreach(Comment reply in replies)

                {
                    if(reply.ArtistId == artistId || comment.CreationDate < thirtyDaysAgo) //Make sure it is not the user, or over 30 days old
                    {
                        continue;
                    }
                    var notification = new Notification
                    {
                        CommentId = reply.Id,
                        ArtId = -1,
                        ArtistId = -1,
                        Type = 3,
                        User = comment.CommenterName,
                        Viewed = comment.Viewed,
                        ArtName = ""
                    };
                    notifications.Add(notification);
                }
            }
            return notifications;
        }
        public async Task<bool> MarkComment(int commentId)
        {
            var connectionString = appConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //Need to Append Created On to query when added to database
                string query =
                    $@"
                          Update Comment
                          Set Viewed = 1
                          Where Id = @commentId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@commentId", commentId);

                    int rowsChanged = await command.ExecuteNonQueryAsync();
                    if (rowsChanged > 0)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Something went wrong with executing the query");
                    }
                }
            }
        }
        public async Task<bool> MarkLike(int artId, int artistId)
        {
            var connectionString = appConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //Need to Append Created On to query when added to database
                string query =
                    $@"
                          Update Likes
                          Set Viewed = 1
                          Where ArtId = @artId and ArtistId = @artistId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@artId", artId);
                    command.Parameters.AddWithValue("@artistId", artistId);

                    int rowsChanged = await command.ExecuteNonQueryAsync();
                    if (rowsChanged > 0)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Something went wrong with executing the query");
                    }
                }
            }
        }
    }
}
