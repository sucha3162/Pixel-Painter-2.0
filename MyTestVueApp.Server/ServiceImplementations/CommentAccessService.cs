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
        private readonly IOptions<ApplicationConfiguration> AppConfig;
        private readonly ILogger<CommentAccessService> Logger;
        public CommentAccessService(IOptions<ApplicationConfiguration> appConfig, ILogger<CommentAccessService> logger)
        {
            AppConfig = appConfig;
            Logger = logger;
        }
        /// <summary>
        /// Gets all comments that relate to an artpiece
        /// </summary>
        /// <param name="id">Id of the artwork that the commetns belong to</param>
        /// <returns>Returns an IEnumerable(array) of Comments</returns>
        public async Task<IEnumerable<Comment>> GetCommentsByArtId(int id)
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
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    Id = reader.GetInt32(0),
                                    ArtistId = reader.GetInt32(1),
                                    ArtId = reader.GetInt32(2),
                                    Message = reader.GetString(3),
                                    CommenterName = reader.GetString(4),
                                    CreationDate = reader.GetDateTime(5),
                                    ReplyId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
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
        /// <summary>
        /// Goes into the database to change the message of a comment made by a user
        /// </summary>
        /// <param name="commentId">Id of the comment to be altered</param>
        /// <param name="newMessage">New text of the comment</param>
        /// <returns>Returns 0 if the Id is not specific, -1, if it fails, or 1+ if it updated the comment</returns>
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

                    int rowsChanged = await command.ExecuteNonQueryAsync();
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
        /// <summary>
        /// Deletes a comment in the database
        /// </summary>
        /// <param name="commentId">Id of the comment to be deleted</param>
        /// <returns>Returns 0 if the Id is not specific enough, -1 if it fails, and 1+ if it removed a comment</returns>
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

                            int rowsChanged = await DeleteResponse.ExecuteNonQueryAsync();
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

                    int rowsChanged = await command.ExecuteNonQueryAsync();
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
        /// <summary>
        /// Creates a comment in the database
        /// </summary>
        /// <param name="commenter">Artist who is leaving a comment</param>
        /// <param name="comment">Comment object being added to the database</param>
        /// <returns>Returns the newly added comment object</returns>
        public async Task<Comment> CreateComment(Artist commenter, Comment comment)
        {
            comment.ArtistId = commenter.Id;
            comment.CommenterName = commenter.Name;
            comment.CreationDate = DateTime.UtcNow;

            using (SqlConnection connection = new SqlConnection(AppConfig.Value.ConnectionString))
            {
                try
                {
                    connection.Open();
                    var insertQuery = "INSERT INTO Comment (ArtistId,ArtId,ReplyId,Message,CreationDate) VALUES (@ArtistID,@ArtID,@replyID,@Message,@CreationDate)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ArtistID", commenter.Id);
                        command.Parameters.AddWithValue("@ArtID", comment.ArtId);
                        command.Parameters.AddWithValue("@replyID", comment.ReplyId);
                        command.Parameters.AddWithValue("@Message", comment.Message);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.UtcNow);

                        var newId = await command.ExecuteNonQueryAsync();
                        comment.Id = Convert.ToInt32(newId);

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commenter"></param>
        /// <param name="comment1"></param>
        /// <param name="comment2"></param>
        /// <returns></returns>
        public async Task<Comment> CreateReply(Artist commenter, Comment comment1, Comment comment2)
        {
            comment1.ArtistId = commenter.Id;
            comment1.CommenterName = commenter.Name;
            comment1.CreationDate = DateTime.UtcNow;
            comment1.ReplyId = comment2.Id;

            using (SqlConnection connection = new SqlConnection(AppConfig.Value.ConnectionString))
            {
                try
                {
                    connection.Open();
                    var insertQuery = "INSERT INTO Comment (ArtistId,ArtId,ReplyId,Message,CreationDate) VALUES (@ArtistID,@ArtID,@replyID,@Message,@CreationDate)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ArtistID", commenter.Id);
                        command.Parameters.AddWithValue("@ArtID", comment1.ArtId);
                        command.Parameters.AddWithValue("@replyID", comment1.ReplyId);
                        command.Parameters.AddWithValue("@Message", comment1.Message);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.UtcNow);

                        var newId = await command.ExecuteNonQueryAsync();
                        comment1.Id = Convert.ToInt32(newId);

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
        /// <summary>
        /// Gets a comment based on it's Id field
        /// </summary>
        /// <param name="id">Id of the comment to be returned</param>
        /// <returns>A Comment</returns>
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
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    Id = reader.GetInt32(0),
                                    ArtistId = reader.GetInt32(1),
                                    ArtId = reader.GetInt32(2),
                                    Message = reader.GetString(3),
                                    CommenterName = reader.GetString(4),
                                    CreationDate = reader.GetDateTime(5),
                                    ReplyId = reader.IsDBNull(6) ? -1 : reader.GetInt32(6)
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
                throw ex;
            }
        }
        /// <summary>
        /// Gets a list of comments made by a specific user
        /// </summary>
        /// <param name="id">Id of the user who made comments</param>
        /// <returns>A list of comments a user made</returns>
        public async Task<IEnumerable<Comment>> GetCommentByUserId(int id)
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
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    Id = reader.GetInt32(0),
                                    ArtistId = reader.GetInt32(1),
                                    ArtId = reader.GetInt32(2),
                                    Message = reader.GetString(3),
                                    CommenterName = reader.GetString(4),
                                    CreationDate = reader.GetDateTime(5),
                                    ReplyId = reader.IsDBNull(6) ? -1 : reader.GetInt32(6)
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
        /// <summary>
        /// Gets a list of Comments that respond to another comment
        /// </summary>
        /// <param name="id">Comment that the returned comments reference</param>
        /// <returns>A list of reply comments</returns>
        public async Task<IEnumerable<Comment>> GetReplyByCommentId(int id)
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
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                { //Art Table + NumLikes and NumComments
                                    Id = reader.GetInt32(0),
                                    ArtistId = reader.GetInt32(1),
                                    ArtId = reader.GetInt32(2),
                                    Message = reader.GetString(3),
                                    CommenterName = reader.GetString(4),
                                    CreationDate = reader.GetDateTime(5),
                                    ReplyId = reader.IsDBNull(6) ? -1 : reader.GetInt32(6),
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

