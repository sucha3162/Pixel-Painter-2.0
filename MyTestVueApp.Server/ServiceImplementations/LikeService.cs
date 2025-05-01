using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace MyTestVueApp.Server.ServiceImplementations
{
    public class LikeService : ILikeService
    {
        private readonly IOptions<ApplicationConfiguration> AppConfig;
        private readonly ILogger<LikeService> Logger;
        public LikeService(IOptions<ApplicationConfiguration> appConfig, ILogger<LikeService> logger)
        {
            AppConfig = appConfig;
            Logger = logger;
        }
        /// <summary>
        /// Insert's into the database what artwork an artist has liked
        /// </summary>
        /// <param name="artId">Id being lliked</param>
        /// <param name="artist">Id of the artist who liked the artwork</param>
        /// <returns>0 if invalid input, -1 if the input failed, and 1+ if it succeeded</returns>
        public async Task<int> InsertLike(int artId, Artist artist)
        {
            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //Check to make sure the user hasnt already liked this work of art
                var checkDupQuery = "SELECT Count(*) FROM Likes WHERE ArtistID = @ArtistId AND ArtID = @ArtId";
                using (SqlCommand checkDupCommand = new SqlCommand(checkDupQuery, connection))
                {
                    checkDupCommand.Parameters.AddWithValue("@ArtistId", artist.Id);
                    checkDupCommand.Parameters.AddWithValue("@ArtId", artId);

                    int count = (int) await checkDupCommand.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        Console.WriteLine("This user has already liked this art piece!");
                        return 0;
                    }
                }

                var query = "INSERT INTO Likes (ArtistID, ArtID, Viewed) VALUES (@ArtistId, @ArtId, 0)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ArtistId", artist.Id);
                    command.Parameters.AddWithValue("@ArtId", artId);

                    int rowsChanged = await command.ExecuteNonQueryAsync();
                    if (rowsChanged > 0)
                    {
                        return rowsChanged;
                    } else {
                        return -1;
                    }
                }
            }
        }
        /// <summary>
        /// Removes the like relation from the database
        /// </summary>
        /// <param name="artId">Artwork being unliked</param>
        /// <param name="artist">Artist who is unliking the artwork</param>
        /// <returns>0 if bad input, -1 if it fails, 1+ if it succeeds</returns>
        public async Task<int> RemoveLike(int artId, Artist artist)
        {
            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //Check to make sure like exists
                 //Check to make sure the user hasnt already liked this work of art
                var checkDupQuery = "SELECT Count(*) FROM Likes WHERE ArtistID = @ArtistId AND ArtID = @ArtId";
                using (SqlCommand checkDupCommand = new SqlCommand(checkDupQuery, connection))
                {
                    checkDupCommand.Parameters.AddWithValue("@ArtistId", artist.Id);
                    checkDupCommand.Parameters.AddWithValue("@ArtId", artId);

                    int count = (int) await checkDupCommand.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        Console.WriteLine("The like you are trying to delete doesnt exist!");
                        return 0;
                    }
                }

                var query = "DELETE FROM Likes WHERE ArtistID = @ArtistId AND ArtID = @ArtId";
                using (SqlCommand command = new SqlCommand(query, connection)) 
                {
                    command.Parameters.AddWithValue("@ArtistId", artist.Id);
                    command.Parameters.AddWithValue("@ArtId", artId);

                    int rowsChanged = await command.ExecuteNonQueryAsync();
                    if (rowsChanged > 0)
                    {
                        return rowsChanged;
                    } else {
                        return -1;
                    }
                }
            }
        }
        /// <summary>
        /// Checks to see if an artwork is liked by the user
        /// </summary>
        /// <param name="artId">Id of the artwork being checked</param>
        /// <param name="artist">Id of the user who would've liked the post</param>
        /// <returns>Returns true if it is liked by the given artist, false otherwise</returns>
        public async Task<bool> IsLiked(int artId, Artist artist) {
            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString)) 
            {
                connection.Open();

                string likedQuery = "SELECT Count(*) FROM Likes WHERE ArtistId = @ArtistId AND ArtID = @ArtID";
                using (SqlCommand command = new SqlCommand(likedQuery, connection))
                {
                    command.Parameters.AddWithValue("@ArtistId", artist.Id);
                    command.Parameters.AddWithValue("@ArtID", artId);

                    int count = (int) await command.ExecuteScalarAsync();

                    if (count > 0) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// Gets all likes an artwork has
        /// </summary>
        /// <param name="artworkId">Id of the artwork being referenced</param>
        /// <returns>A list of Like objects</returns>
        public async Task<IEnumerable<Like>> GetLikesByArtwork(int artworkId)
        {
            var likes = new List<Like>();
            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //Need to Append Created On to query when added to database
                string likedQuery = 
                    $@"
                        SELECT Artist.Name, Art.Title, Likes.ArtId, Likes.ArtistId, Likes.Viewed 
                        FROM Likes
                        LEFT JOIN Art ON Art.ID = Likes.ArtID 
                        LEft join Artist on Artist.Id = Likes.ArtistId
                        WHERE Likes.ArtId = @artworkId";
                using (SqlCommand command = new SqlCommand(likedQuery, connection))
                {
                    command.Parameters.AddWithValue("@artworkId", artworkId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var like = new Like
                            {   //ArtId, ArtName
                                Artist = reader.GetString(0),
                                Artwork = reader.GetString(1),
                                ArtId = reader.GetInt32(2),
                                ArtistId = reader.GetInt32(3),
                                Viewed = reader.GetInt32(4) == 1 ? true : false,
                                LikedOn = new DateTime()
                            };
                            likes.Add(like);
                        }
                    }
                }
            }
            return likes;
        }
        /// <summary>
        /// Gets the Like object that belong to the artist and artwork referenced
        /// </summary>
        /// <param name="artId">Id of the art being checked</param>
        /// <param name="artistId">Id of the artist who would've made the like</param>
        /// <returns>A Like object if found, null otherwise</returns>
        public async Task<Like> GetLikeByIds(int artId, int artistId)
        {
            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //Need to Append Created On to query when added to database
                string likedQuery =
                    $@"
                          SELECT Artist.Name, Art.Title, Likes.ArtId, Likes.ArtistId, Likes.Viewed 
                          FROM Likes
                          LEFT JOIN Art ON Art.ID = Likes.ArtID 
                          LEFT JOIN Artist ON Likes.ArtistId = Artist.Id
                          WHERE Likes.ArtId = @art and Likes.ArtistId = @artist
                          ";
                using (SqlCommand command = new SqlCommand(likedQuery, connection))
                {
                    command.Parameters.AddWithValue("@artist", artistId);
                    command.Parameters.AddWithValue("@art", artId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var like = new Like
                            {   //ArtId, ArtName
                                Artist = reader.GetString(0),
                                Artwork = reader.GetString(1),
                                ArtId = reader.GetInt32(2),
                                ArtistId = reader.GetInt32(3),
                                Viewed = reader.GetInt32(4) == 1 ? true : false,
                                LikedOn = new DateTime()
                            };
                            return like;
                        }
                    }
                }
            }
            throw new ArgumentException("No like data in the datbase matches values art id: " + artId + " and artist id: " + artistId);
        }
    }
}