using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.ComponentModel.Design;
using System.Xml.Linq;


namespace MyTestVueApp.Server.ServiceImplementations
{
    public class CommentAccessService : ICommentAccessService
    {
        private IOptions<ApplicationConfiguration> AppConfig { get; }
        private ILogger<CommentAccessService> Logger { get; }
        public CommentAccessService(IOptions<ApplicationConfiguration> appConfig, ILogger<CommentAccessService> logger)
        {
            AppConfig = appConfig;
            Logger = logger;
        }
        public IEnumerable<Comment> GetCommentsByArtId(int id)
        {
            try
            {
                var comments = new List<Comment>();
                var connectionString = AppConfig.Value.ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                    var query = @$"
                   SELECT 
                        Comment.Id, 
                        Comment.ArtistId, 
                        Comment.ArtID, 
                        Comment.[Message],
	                    Artist.[Name] as CommenterName,
                        Comment.CreationDate,
                        Comment.ReplyId,
                        Comment.Viewed
                    FROM Comment  
                    JOIN Artist ON Artist.id = Comment.ArtistId
                    WHERE ArtID = @id
                    Order By CreationDate DESC;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@id", id));
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    id = reader.GetInt32(0),
                                    artistId = reader.GetInt32(1),
                                    artId = reader.GetInt32(2),
                                    message = reader.GetString(3),
                                    commenterName = reader.GetString(4),
                                    creationDate = reader.GetDateTime(5),
                                    replyId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                    Viewed = reader.GetInt32(7)==0 ? false : true
                                };
                                comments.Add(comment);
                            }
                        }
                    }
                }
                return comments;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error retrieving comments");
                throw;
            }
        }
        public async Task<int> EditComment(int commentId, string newMessage)
        {
            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //Check to make sure the user hasnt already liked this work of art
                var checkDupQuery = "SELECT Count(*) FROM comment WHERE ID = @CommentId";
                using (SqlCommand checkDupCommand = new SqlCommand(checkDupQuery, connection))
                {
                    checkDupCommand.Parameters.AddWithValue("@CommentId", commentId);

                    int count = (int)await checkDupCommand.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        Console.WriteLine("No comment exists to edit!");
                        return 0;
                    }
                    if (count > 1)
                    {
                        Console.WriteLine("more than one comment exists to edit!");
                        return 0;
                    }
                }
                //update table here
                var query = "UPDATE Comment SET Message = @newComment WHERE ID = @commentID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@newComment", newMessage);
                    command.Parameters.AddWithValue("@CommentID", commentId);

                    int rowsChanged = command.ExecuteNonQuery();
                    if (rowsChanged > 0)
                    {
                        Console.WriteLine("Comment was changed sucessfully!");
                        return rowsChanged;
                    }
                    else
                    {
                        Console.WriteLine("Failed to edit comment!");
                        return -1;
                    }
                }
            }
        }
        
        public async Task<int> DeleteComment(int commentId)
        {
            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //Check to make sure the there is a comment to delete
                var checknullQuery = "SELECT Count(*) FROM comment WHERE ID = @CommentId";
                using (SqlCommand checkDupCommand = new SqlCommand(checknullQuery, connection))
                {
                    checkDupCommand.Parameters.AddWithValue("@CommentId", commentId);

                    int count = (int)await checkDupCommand.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        Console.WriteLine("No comment exists to delete!");
                        return 0;
                    }
                    if (count > 1)
                    {
                        Console.WriteLine("more than one comment with same ID!");
                        return 0;
                    }
                }
                var checkResponseQuery = "SELECT Count(*) FROM comment WHERE ReplyId = @CommentId";
                using (SqlCommand checkResponseCommand = new SqlCommand(checknullQuery, connection))
                {
                    checkResponseCommand.Parameters.AddWithValue("@CommentId", commentId);

                    int count = (int)await checkResponseCommand.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        var subquery = "DELETE Comment WHERE ReplyId = @commentId";
                        using (SqlCommand DeleteResponse = new SqlCommand(subquery, connection))
                        {
                            DeleteResponse.Parameters.AddWithValue("@CommentId", commentId);

                            int rowsChanged = DeleteResponse.ExecuteNonQuery();
                            if (rowsChanged > 0)
                            {
                                Console.WriteLine("Response Comments Deleted");
                            }
                        }
                    }
                }
                //update table here
                var query = "Delete Comment WHERE ID = @CommentId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CommentId", commentId);

                    int rowsChanged = command.ExecuteNonQuery();
                    if (rowsChanged > 0)
                    {
                        Console.WriteLine("Comment was Deleted sucessfully!");
                        return rowsChanged;
                    }
                    else
                    {
                        Console.WriteLine("Failed to edit comment!");
                        return -1;
                    }
                }
            }

        }

        public async Task<Comment> CreateComment(Artist commenter, Comment comment)
        {
            comment.artistId = commenter.id;
            comment.commenterName = commenter.name;
            comment.creationDate = DateTime.UtcNow;

            using (SqlConnection connection = new SqlConnection(AppConfig.Value.ConnectionString))
            {
                try
                {
                    connection.Open();
                    var insertQuery = "INSERT INTO Comment (ArtistId,ArtId,ReplyId,Message,CreationDate) VALUES (@ArtistID,@ArtID,@replyID,@Message,@CreationDate)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ArtistID", commenter.id);
                        command.Parameters.AddWithValue("@ArtID", comment.artId);
                        command.Parameters.AddWithValue("@replyID", comment.replyId);
                        command.Parameters.AddWithValue("@Message", comment.message);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.UtcNow);

                        var newId = await command.ExecuteNonQueryAsync();
                        comment.id = Convert.ToInt32(newId);

                        return comment;

                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "Failed to insert comment");
                    throw;
                }
            }

        }

        public async Task<Comment> CreateReply(Artist commenter, Comment comment1, Comment comment2)
        {
            comment1.artistId = commenter.id;
            comment1.commenterName = commenter.name;
            comment1.creationDate = DateTime.UtcNow;
            comment1.replyId = comment2.id;

            using (SqlConnection connection = new SqlConnection(AppConfig.Value.ConnectionString))
            {
                try
                {
                    connection.Open();
                    var insertQuery = "INSERT INTO Comment (ArtistId,ArtId,ReplyId,Message,CreationDate) VALUES (@ArtistID,@ArtID,@replyID,@Message,@CreationDate)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ArtistID", commenter.id);
                        command.Parameters.AddWithValue("@ArtID", comment1.artId);
                        command.Parameters.AddWithValue("@replyID", comment1.replyId);
                        command.Parameters.AddWithValue("@Message", comment1.message);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.UtcNow);

                        var newId = await command.ExecuteNonQueryAsync();
                        comment1.id = Convert.ToInt32(newId);

                        return comment1;

                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "Failed to insert comment");
                    throw;
                }
            }

        }

        public IEnumerable<Comment> GetCommentByReplyId(int replyId)
        {
            try
            {
                var comments = new List<Comment>();
                var connectionString = AppConfig.Value.ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                    var query = @$"
                   SELECT 
                        Comment.Id, 
                        Comment.ArtistId, 
                        Comment.ArtID, 
                        Comment.[Message],
	                    Artist.[Name] as CommenterName,
                        Comment.CreationDate,
                        Comment.ReplyId
                    FROM Comment  
                    JOIN Artist ON Artist.id = Comment.ArtistId
                    WHERE Comment.ReplyId = @replyID;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@id", replyId));
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Comment comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    id = reader.GetInt32(0),
                                    artistId = reader.GetInt32(1),
                                    artId = reader.GetInt32(2),
                                    message = reader.GetString(3),
                                    commenterName = reader.GetString(4),
                                    creationDate = reader.GetDateTime(5),
                                    replyId = reader.IsDBNull(6) ? -1 : reader.GetInt32(6)
                                };
                                comments.Add(comment);
                            }
                        }
                    }
                    return comments;
                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error retrieving replies");
                throw;
            }
        }
        public async Task<Comment> GetCommentByCommentId(int id)
        {
            try
            {
                Comment comments = new Comment();
                var connectionString = AppConfig.Value.ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                    var query = @$"
                   SELECT 
                        Comment.Id, 
                        Comment.ArtistId, 
                        Comment.ArtID, 
                        Comment.[Message],
	                    Artist.[Name] as CommenterName,
                        Comment.CreationDate,
                        Comment.ReplyId
                    FROM Comment  
                    JOIN Artist ON Artist.id = Comment.ArtistId
                    WHERE Comment.Id = @id;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@id", id));
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    id = reader.GetInt32(0),
                                    artistId = reader.GetInt32(1),
                                    artId = reader.GetInt32(2),
                                    message = reader.GetString(3),
                                    commenterName = reader.GetString(4),
                                    creationDate = reader.GetDateTime(5),
                                    replyId = reader.IsDBNull(6) ? -1 : reader.GetInt32(6)
                                };
                                comments = comment;
                            }
                        }
                    }
                }
                return comments;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error retrieving comments");
                throw;
            }
        }
        public IEnumerable<Comment> GetCommentByUserId(int id)
        {
            try
            {

                List<Comment> comments = new List<Comment>();
                var connectionString = AppConfig.Value.ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                    var query = @$"
                   SELECT 
                        Comment.Id, 
                        Comment.ArtistId, 
                        Comment.ArtID, 
                        Comment.[Message],
	                    Artist.[Name] as CommenterName,
                        Comment.CreationDate,
                        Comment.ReplyId
                    FROM Comment  
                    JOIN Artist ON Artist.id = Comment.ArtistId
                    WHERE Comment.ArtistId = @id;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@id", id));
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    id = reader.GetInt32(0),
                                    artistId = reader.GetInt32(1),
                                    artId = reader.GetInt32(2),
                                    message = reader.GetString(3),
                                    commenterName = reader.GetString(4),
                                    creationDate = reader.GetDateTime(5),
                                    replyId = reader.IsDBNull(6) ? -1 : reader.GetInt32(6)
                                };
                                comments.Add(comment);
                            }
                        }
                    }
                }
                return comments;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error retrieving comments");
                throw;
            }
        }
        public IEnumerable<Comment> GetReplyByCommentId(int id)
        {
            try
            {
                List<Comment> comments = new List<Comment>();
                var connectionString = AppConfig.Value.ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                    var query = @$"
                   SELECT 
                        Comment.Id, 
                        Comment.ArtistId, 
                        Comment.ArtID, 
                        Comment.[Message],
	                    Artist.[Name] as CommenterName,
                        Comment.CreationDate,
                        Comment.ReplyId,
                        Comment.Viewed
                    FROM Comment  
                    JOIN Artist ON Artist.id = Comment.ArtistId
                    WHERE Comment.ReplyId = @id;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@id", id));
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    id = reader.GetInt32(0),
                                    artistId = reader.GetInt32(1),
                                    artId = reader.GetInt32(2),
                                    message = reader.GetString(3),
                                    commenterName = reader.GetString(4),
                                    creationDate = reader.GetDateTime(5),
                                    replyId = reader.IsDBNull(6) ? -1 : reader.GetInt32(6),
                                    Viewed = reader.GetInt32(7) == 0 ? false : true
                                };
                                comments.Add(comment);
                            }
                        }
                    }
                }
                return comments;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error retrieving comments");
                throw;
            }
        }
    }
}

