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
        private IOptions<ApplicationConfiguration> AppConfig { get; }
        private ILogger<LoginService> Logger { get; }

        private string[] adjectives = new string[]
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

        private string[] nouns = new string[]
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

                        int rowsChanged = command.ExecuteNonQuery();
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

        private string getAdjective(int index)
        {
            return adjectives[index];
        }

        private string getNoun(int index)
        {
            return nouns[index];
        }

        private string generateUsername()
        {
            Random rnd = new Random();

            var username = getAdjective(rnd.Next(50)) + getNoun(rnd.Next(50)) + rnd.Next(1000);

            return username;
        }

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

                        int count = (int)await checkDupCommand.ExecuteScalarAsync();
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

                        int rowsChanged = command.ExecuteNonQuery();
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
                                id = reader.GetInt32(0),
                                subId = reader.GetString(1),
                                name = reader.GetString(2),
                                isAdmin = reader.GetBoolean(3),
                                creationDate = reader.GetDateTime(4),
                                email = reader.GetString(5),
                            };
                            return artist;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<bool> IsUserAdmin(string userId)
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

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            artist = new Artist
                            {
                                id = reader.GetInt32(0),
                                subId = reader.GetString(1),
                                name = reader.GetString(2),
                                isAdmin = reader.GetBoolean(3),
                                creationDate = reader.GetDateTime(4),
                                email = reader.GetString(5),
                            };
                            return artist.isAdmin;
                        }
                    }
                }
            }
            return false;
        }
        public async Task DeleteArtist(int ArtistId)
        {
            try
            {
                var connectionString = AppConfig.Value.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var deleteArtQuery = "DELETE artist where artist.id = @ArtistId";
                    using (SqlCommand deleteArtCommand = new SqlCommand(deleteArtQuery, connection))
                    {
                        deleteArtCommand.Parameters.AddWithValue("@ArtistId", ArtistId);
                        deleteArtCommand.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error in DeleteArt");
                throw;
            }
        }
    }
}
