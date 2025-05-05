using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using System;
using System.Data;

namespace MyTestVueApp.Server.ServiceImplementations
{
    public class LoginService : ILoginService
    {
        private readonly IOptions<ApplicationConfiguration> AppConfig;
        private readonly ILogger<LoginService> Logger;

        private string[] Adjectives = new string[]
        {
            "Happy", "Sad", "Bright", "Dark", "Quick",
            "Slow", "Loud", "Soft", "Smooth", "Rough",
            "Imploding", "Cold", "New", "Old", "Rich",
            "Poor", "Clean", "Dirty", "Easy", "Hard",
            "Funny", "Serious", "Friendly", "Angry", "Calm",
            "Bitter", "Sweet", "Sour", "Spicy", "Salty",
            "Beautiful", "Ugly", "Strong", "Weak", "Heavy",
            "Light", "Tall", "Short", "Wide", "Narrow",
            "Colorful", "Dull", "Busy", "Quiet", "Famous",
            "Unknown", "Happy", "Lazy", "Active", "Thoughtful"
        };

        private string[] Nouns = new string[]
        {
            "Apple", "Ball", "Cat", "Dog", "Elephant",
            "Flower", "Guitar", "House", "Ice", "Jacket",
            "Kite", "Lion", "Mountain", "Notebook", "Orange",
            "Pencil", "Quilt", "Rocket", "Sun", "Tree",
            "Umbrella", "Violin", "Window", "Car", "Yacht",
            "Zebra", "Book", "Car", "Desk", "Egg",
            "Fish", "Glove", "Hat", "Island", "Jam",
            "Key", "Lamp", "Mug", "Nut", "Oven",
            "Pizza", "Quokka", "Ring", "Shoes", "Turtle",
            "Vase", "Whistle", "Dancer", "Yarn", "Zipper"
        };

        public LoginService(IOptions<ApplicationConfiguration> appConfig, ILogger<LoginService> logger)
        {
            AppConfig = appConfig;
            Logger = logger;
        }

        #region Google Login

        public async Task<Userinfo> GetUserId(string code)
        {
            var accessToken = await GetAccessToken(code);

            try
            {
                var credential = GoogleCredential.FromAccessToken(accessToken);
                var oauth2Service = new Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "MyTestVueApp"
                });

                Userinfo userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();

                return userInfo;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error in GetUserEmail");
                throw;
            }
        }

        private async Task<string> GetAccessToken(string code)
        {
            try
            {
                var tokenResponse = new AuthorizationCodeTokenRequest
                {
                    ClientId = AppConfig.Value.ClientId,
                    ClientSecret = AppConfig.Value.ClientSecret,
                    Code = code,
                    GrantType = "authorization_code",
                    RedirectUri = AppConfig.Value.HomeUrl + "login/LoginRedirect"
                };

                var result = await tokenResponse.ExecuteAsync(new HttpClient(), GoogleAuthConsts.TokenUrl, CancellationToken.None, SystemClock.Default);
                return result.AccessToken;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error in GetSubId");
                throw;
            }
        }

        /// <summary>
        /// Runs on first login to add the user to the database
        /// </summary>
        /// <param name="subId"></param>
        /// <returns></returns>
        public async Task<Artist> SignupActions(string subId, string email)
        {
            try
            {
                var connectionString = AppConfig.Value.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var artist = await GetUserBySubId(subId);

                    if (artist != null)
                    {
                        return artist;
                    }

                    var query = "INSERT INTO Artist (SubId, Name, Email, IsAdmin, CreationDate) VALUES (@SubId, @Name, @Email, @IsAdmin, @CreationDate)";
                    string username = generateUsername();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SubId", subId);
                        command.Parameters.AddWithValue("@Name", username);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@IsAdmin", false);
                        command.Parameters.AddWithValue("@CreationDate", DateTime.UtcNow);

                        int rowsChanged = await command.ExecuteNonQueryAsync();
                        if (rowsChanged > 0)
                        {
                            return await GetUserBySubId(subId);
                        }
                        else
                        {
                            throw new Exception("No rows changed during signup actions");
                        }             
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Signup actions failed!");
                throw;
            }
        }

        #endregion
        /// <summary>
        /// Used for grabbing a certain adjective in the array of adjectives
        /// </summary>
        /// <param name="index">Location of the adjective to grab</param>
        /// <returns>An adjective string</returns>
        private string getAdjective(int index)
        {
            return Adjectives[index];
        }
        /// <summary>
        /// Used for grabbing specfic a specfic noun in the array of nouns
        /// </summary>
        /// <param name="index">Location of the noun to grab</param>
        /// <returns>A noun string</returns>
        private string getNoun(int index)
        {
            return Nouns[index];
        }
        /// <summary>
        /// Generates a random username for the user through random number generation
        /// </summary>
        /// <returns>A new randomly generated username</returns>
        private string generateUsername()
        {
            Random rnd = new Random();

            var username = getAdjective(rnd.Next(50)) + getNoun(rnd.Next(50)) + rnd.Next(1000);

            return username;
        }
        /// <summary>
        /// Queues all artists in the database and returns them as a list of artists
        /// </summary>
        /// <returns>A List of Artists</returns>
        public async Task<IEnumerable<Artist>> GetAllArtists()
        {
            var artistList = new List<Artist>();
            var connectionString = AppConfig.Value.ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query =
                    @"SELECT [Id]
                        ,[SubId] 
                        ,[Name] 
                        ,[IsAdmin]
						,[PrivateProfile]
                        ,[CreationDate] 
                    FROM [PixelPainter].[dbo].[Artist]
                    ";
                
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var artist = new Artist
                            {
                                Id = reader.GetInt32(0),
                                SubId = reader.GetString(1),
                                Name = reader.GetString(2),
                                IsAdmin = reader.GetBoolean(3),
                                PrivateProfile = reader.GetBoolean(4),
                                CreationDate = reader.GetDateTime(5)
                            };
                            artistList.Add(artist);
                        }
                    }
                    return artistList;
                }
            }
        }
        /// <summary>
        /// This Function changes the PrivateProfile value, making it the inverse of the initial value
        /// </summary>
        /// <param name="artistId"> ID of the artist</param>
        /// <returns>A true when changing from public to private or false when changing from private to public</returns>
        public async Task<bool> PrivateSwitchChange(int artistId)
        {
            var artist = await GetArtistById(artistId);
            var connectionString = AppConfig.Value.ConnectionString;
            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (artist.PrivateProfile)
                {
                    var query =
                        @"update Artist
                          set PrivateProfile = 0
                          where Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", artist.Id);
                        command.Parameters.AddWithValue("@PrivateProfile", artist.PrivateProfile);
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
                else
                {
                    var query =
                        @"update Artist
                          set PrivateProfile = 1
                          where Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", artist.Id);
                        command.Parameters.AddWithValue("@PrivateProfile", artist.PrivateProfile);
                        await command.ExecuteNonQueryAsync();
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// This function intakes a new username and SubID to identify user 
        /// and changes their name on the backend in the database
        /// </summary>
        /// <param name="newUsername">New username being passed in</param>
        /// <param name="subId"> SubID of user </param>
        /// <returns>Returns true if the name is changed and false if the name is not changed</returns>
        public async Task<bool> UpdateUsername(string newUsername, string subId)
        {
            try
            {
                var connectionString = AppConfig.Value.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // This query and command is to check if the username is already taken or not
                    var checkDupQuery = "SELECT COUNT(*) FROM Artist WHERE Name = @Name";
                    using (SqlCommand checkDupCommand = new SqlCommand(checkDupQuery, connection))
                    {
                        checkDupCommand.Parameters.AddWithValue("@Name", newUsername);

                        int count = (int) await checkDupCommand.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            return false;
                        }
                    }

                    var query = "UPDATE Artist SET Name = @Name WHERE SubId = @SubId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SubId", subId);
                        command.Parameters.AddWithValue("@Name", newUsername);

                        int rowsChanged = await command.ExecuteNonQueryAsync();
                        if (rowsChanged > 0)
                        {
                            return true;
                        }
                        else
                        {
                            throw new Exception("Failed to update username");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error occured while updating username");
                throw;
            }
        }
        /// <summary>
        /// Gets an artist from the database, based on the name od the user
        /// </summary>
        /// <param name="name">name of the artist</param>
        /// <returns>Retuends an artist object</returns>
        public async Task<Artist> GetArtistByName(string name)
        {
            var artist = new Artist();
            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = $@" 
                  SELECT [Id]
                    ,[SubId]
                    ,[Name]
                    ,[IsAdmin]
                    ,[CreationDate]
                    ,[PrivateProfile]
                FROM [PixelPainter].[dbo].[Artist]
                where Artist.Name = @Name
                    ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {

                            artist = new Artist
                            {
                                Id = reader.GetInt32(0),
                                SubId = reader.GetString(1),
                                Name = reader.GetString(2),
                                IsAdmin = reader.GetBoolean(3),
                                CreationDate = reader.GetDateTime(4),
                                PrivateProfile = reader.GetBoolean(5),

                            };
                            
                            return artist;
                        }
                    }
                    return null;
                }
            }
        }
        /// <summary>
        /// Gets a user from the database based on their google OAuth Id
        /// </summary>
        /// <param name="subId">Id of the artist to return</param>
        /// <returns>An artist object</returns>
        public async Task<Artist> GetUserBySubId(string subId)
        {
            var artist = new Artist();

            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT TOP (1) [Id]
                          ,[SubId]
                          ,[Name]
                          ,[IsAdmin]
                          ,[CreationDate]
                          ,[PrivateProfile]
                          ,[Email]
                      FROM [PixelPainter].[dbo].[Artist]
                      WHERE SubId = @SubId
                    ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SubId", subId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            artist = new Artist
                            {
                                Id = reader.GetInt32(0),
                                SubId = reader.GetString(1),
                                Name = reader.GetString(2),
                                IsAdmin = reader.GetBoolean(3),
                                CreationDate = reader.GetDateTime(4),
                                PrivateProfile = reader.GetBoolean(5),
                                Email = reader.GetString(6),
                            };
                            return artist;
                        }
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Checks the database to see if a user is an admin or not
        /// </summary>
        /// <param name="userId">Id of the user being checked</param>
        /// <returns>True if the user is an admin, false otherwise</returns>
        public async Task<bool> IsUserAdmin(string userId)
        {
            var artist = new Artist();

            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT IsAdmin
                      FROM Artist
                      WHERE Id = @Id
                    ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            artist = new Artist
                            {
                                Id = reader.GetInt32(0),
                                SubId = reader.GetString(1),
                                Name = reader.GetString(2),
                                IsAdmin = reader.GetBoolean(3),
                                CreationDate = reader.GetDateTime(4),
                                Email = reader.GetString(5),
                            };
                            return artist.IsAdmin;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Removes an artist from the databaes
        /// </summary>
        /// <param name="ArtistId">Id of the artist to be removed</param>
        public async void DeleteArtist(int ArtistId)
        {
            try
            {
                var connectionString = AppConfig.Value.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var deleteArtQuery = "DELETE artist where artist.id =  @ArtistId ";
                    using (SqlCommand deleteArtCommand = new SqlCommand(deleteArtQuery, connection))
                    {
                        deleteArtCommand.Parameters.AddWithValue("@ArtistId", ArtistId);
                        await deleteArtCommand.ExecuteNonQueryAsync();
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error in DeleteArt");
                throw;
            }
        }
        /// <summary>
        /// Gets a user from the database based on their Id
        /// </summary>
        /// <param name="id">Id of the artist being retrieved</param>
        /// <returns>An artist object</returns>
        public async Task<Artist> GetArtistById(int id)
        {
            var artist = new Artist();

            var connectionString = AppConfig.Value.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT TOP (1) [Id]
                          ,[SubId]
                          ,[Name]
                          ,[IsAdmin]
                          ,[CreationDate]
                          ,[Email]
                      FROM [PixelPainter].[dbo].[Artist]
                      WHERE Id = @Id
                    ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            artist = new Artist
                            {
                                Id = reader.GetInt32(0),
                                SubId = reader.GetString(1),
                                Name = reader.GetString(2),
                                IsAdmin = reader.GetBoolean(3),
                                CreationDate = reader.GetDateTime(4),
                                Email = reader.GetString(5),
                            };
                            return artist;
                        }
                    }
                }
            }
            return null;
        }
    }
}
