using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace MyTestVueApp.Server.ServiceImplementations
{
    public class ArtAccessService : IArtAccessService
    {
        private readonly IOptions<ApplicationConfiguration> AppConfig; 
        private readonly ILogger<ArtAccessService> Logger;
        private readonly ILoginService LoginService;
        public ArtAccessService(IOptions<ApplicationConfiguration> appConfig, ILogger<ArtAccessService> logger, ILoginService loginService)
        {
            AppConfig = appConfig;
            Logger = logger;
            LoginService = loginService;
        }

        public async Task<IEnumerable<Art>> GetAllArt()
        {
            var paintings = new List<Art>();
            var connectionString = AppConfig.Value.ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query1 =
                    @"
                    Select 
	                    Art.Id, 
	                    Art.Title,   
	                    Art.Width, 
	                    Art.Height, 
	                    Art.Encode, 
	                    Art.CreationDate, 
	                    Art.isPublic, 
	                    COUNT(distinct Likes.ArtistId) as Likes, 
	                    Count(distinct Comment.Id) as Comments  
                    FROM ART  
	                    LEFT JOIN Likes ON Art.ID = Likes.ArtID  
	                    LEFT JOIN Comment ON Art.ID = Comment.ArtID  
                    GROUP BY Art.ID, Art.Title, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic;
                    ";

                using (var command = new SqlCommand(query1, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var pixelGrid = new PixelGrid()
                            {
                                Width = reader.GetInt32(2),
                                Height = reader.GetInt32(3),
                                EncodedGrid = reader.GetString(4)
                            };
                            var painting = new Art
                            { //Art Table + NumLikes and NumComments
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                CreationDate = reader.GetDateTime(5),
                                IsPublic = reader.GetBoolean(6),
                                NumLikes = reader.GetInt32(7),
                                NumComments = reader.GetInt32(8),
                                PixelGrid = pixelGrid,
                            };

                            painting.SetArtists((await GetArtistsByArtId(painting.Id)).ToList());
                            paintings.Add(painting);
                        }
                    }
                }
            }
            return paintings;
        }
        public async Task<Art> GetArtById(int id)
        {
            var connectionString = AppConfig.Value.ConnectionString;
            var painting = new Art();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                var query =
                    $@"
                    Select	
	                    Art.ID,
	                    Art.Title, 
	                    Art.Width, 
	                    Art.Height, 
	                    Art.Encode, 
	                    Art.CreationDate,
	                    Art.isPublic,
	                    COUNT(distinct Likes.ArtistId) as Likes, 
	                    Count(distinct Comment.Id) as Comments  
                    FROM ART  
                    LEFT JOIN Likes ON Art.ID = Likes.ArtID  
                    LEFT JOIN Comment ON Art.ID = Comment.ArtID  
                    LEFT JOIN ContributingArtists ON Art.Id = ContributingArtists.ArtId
                    WHERE Art.ID = @artId 
                    GROUP BY Art.ID, Art.Title, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic;
                    ";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@artId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {

                            var pixelGrid = new PixelGrid()
                            {
                                Width = reader.GetInt32(2),
                                Height = reader.GetInt32(3),
                                EncodedGrid = reader.GetString(4)
                            };
                            painting = new Art
                            { //ArtId, ArtName, Width, ArtLength, Encode, Date, IsPublic
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                PixelGrid = pixelGrid,
                                CreationDate = reader.GetDateTime(5),
                                IsPublic = reader.GetBoolean(6),
                                NumLikes = reader.GetInt32(7),
                                NumComments = reader.GetInt32(8)
                            };
                            painting.SetArtists((await GetArtistsByArtId(painting.Id)).ToList());
                            return painting;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Art>> GetArtByArtist(int artistId)
        {
            var paintings = new List<Art>();
            var connectionString = AppConfig.Value.ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                var query =
                    $@"
                      Select	
                        Art.ID,
                        Art.Title,
                        Art.Width, 
	                    Art.Height, 
	                    Art.Encode, 
	                    Art.CreationDate,
	                    Art.isPublic
                      FROM ART
                      left join  ContributingArtists as CA on CA.ArtId = Art.Id
                      WHERE CA.ArtistId = @ArtistID
                    ";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ArtistID", artistId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var pixelGrid = new PixelGrid()
                            {
                                width = reader.GetInt32(2),
                                height = reader.GetInt32(3),
                                encodedGrid = reader.GetString(4)
                            };
                            var painting = new Art
                            {   //ArtId, ArtName
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                PixelGrid = pixelGrid,
                                CreationDate = reader.GetDateTime(5),
                                IsPublic = reader.GetBoolean(6)
                            };
                            paintings.Add(painting);
                        }
                    }
                }
                return paintings;
            }
        }

        public async Task<IEnumerable<Art>> GetLikedArt(int artistId)
        {
            var paintings = new List<Art>();
            var connectionString = AppConfig.Value.ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query =
                    $@"
                    SELECT
	                      Likes.ArtId,
                          Art.Title,
	                      Art.Width, 
	                      Art.Height, 
	                      Art.Encode, 
	                      Art.CreationDate,
	                      Art.isPublic,
	                      COUNT(distinct Likes.ArtistId) as Likes, 
	                      Count(distinct Comment.Id) as Comments
                      FROM Likes
                      left join Art on Art.Id = Likes.ArtId
                      LEFT JOIN Comment ON Art.ID = Comment.ArtID
                      where Likes.ArtistId = @ArtistId
                      GROUP BY Likes.ArtistId, Likes.ArtId, Art.ID, Art.Title, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic;
                        ";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ArtistID", artistId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var pixelGrid = new PixelGrid()
                            {
                                width = reader.GetInt32(2),
                                height = reader.GetInt32(3),
                                encodedGrid = reader.GetString(4)
                            };
                            var painting = new Art
                            {   //ArtId, ArtName
                                id = reader.GetInt32(0),
                                title = reader.GetString(1),
                                pixelGrid = pixelGrid,
                                creationDate = reader.GetDateTime(5),
                                isPublic = reader.GetBoolean(6),
                                numLikes = reader.GetInt32(7),
                                numComments = reader.GetInt32(8)
                            };
                            paintings.Add(painting);
                        }
                    }
                    return paintings;
                }
            }
        }
        public async Task<Art> SaveNewArt(Artist artist, Art art) //Single Artist
        {
            try
            {
                art.CreationDate = DateTime.UtcNow;

                using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                {
                    connection.Open();

                    var query = @"
                    INSERT INTO Art (Title, Width, Height, Encode, CreationDate, IsPublic)
                    VALUES (@Title, @Width, @Height, @Encode, @CreationDate, @IsPublic);
                    SELECT SCOPE_IDENTITY();
                    INSERT INTO ContributingArtists(ArtId,ArtistId) values (@@IDENTITY,@ArtistId);
                ";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Title", art.Title);
                            command.Parameters.AddWithValue("@ArtistId", artist.Id);
                            command.Parameters.AddWithValue("@Width", art.PixelGrid.Width);
                            command.Parameters.AddWithValue("@Height", art.PixelGrid.Height);
                            command.Parameters.AddWithValue("@Encode", art.PixelGrid.EncodedGrid);
                            command.Parameters.AddWithValue("@CreationDate", art.CreationDate);
                            command.Parameters.AddWithValue("@IsPublic", art.IsPublic);

                            var newArtId = await command.ExecuteScalarAsync();
                            art.Id = Convert.ToInt32(newArtId);
                        }
                    }

                return art;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in SaveArt");
                throw;
            }
        }
        public async Task<Art> SaveNewArtMulti(Art art)//Multi artist
        {
            try
            {
                art.CreationDate = DateTime.UtcNow;

                using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                {
                    connection.Open();

                    var query = @"
                    INSERT INTO Art (Title, Width, Height, Encode, CreationDate, IsPublic)
                    VALUES (@Title, @Width, @Height, @Encode, @CreationDate, @IsPublic);
                    SELECT SCOPE_IDENTITY();
                ";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Title", art.Title);
                        command.Parameters.AddWithValue("@Width", art.PixelGrid.Width);
                        command.Parameters.AddWithValue("@Height", art.PixelGrid.Height);
                        command.Parameters.AddWithValue("@Encode", art.PixelGrid.EncodedGrid);
                        command.Parameters.AddWithValue("@CreationDate", art.CreationDate);
                        command.Parameters.AddWithValue("@IsPublic", art.IsPublic);

                        var newArtId = await command.ExecuteScalarAsync();
                        art.Id = Convert.ToInt32(newArtId);
                    }
                }

                return art;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in SaveArt");
                throw;
            }
        }

        public async void AddContributingArtist(int artId, int artistId)
        {
            try
            {
                using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                {
                    connection.Open();

                    var query = @"
                    INSERT INTO ContributingArtists(ArtId,ArtistId) values (@ArtId,@ArtistId);
                ";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ArtId", artId);
                        command.Parameters.AddWithValue("@ArtistId", artistId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in SaveContributingArtist");
                throw;
            }
        }
        public async Task<Art> UpdateArt(Artist artist, Art art)
        {
            try
            {
                var oldArt = GetArtById(art.Id);
                if (oldArt == null)
                {
                    return null;
                }
                else
                {
                    using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                    {
                        connection.Open();

                        var query = @"
                            UPDATE Art SET
	                            Title = @Title,
	                            IsPublic = @IsPublic,
	                            Width = @Width,
	                            Height = @Height,
	                            Encode = @Encode
                            WHERE Id = @Id;
                        ";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Title", art.Title);
                            command.Parameters.AddWithValue("@IsPublic", art.IsPublic);
                            command.Parameters.AddWithValue("@Width", art.PixelGrid.Width);
                            command.Parameters.AddWithValue("@Height", art.PixelGrid.Height);
                            command.Parameters.AddWithValue("@Encode", art.PixelGrid.EncodedGrid);
                            command.Parameters.AddWithValue("@Id", art.Id);

                            await command.ExecuteScalarAsync();
                            
                            return await GetArtById(Convert.ToInt32(art.Id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in UpdateArt");
                throw;
            }

        }

        public async void DeleteArt(int ArtId) //change to admin only and have it so users can remove themselves from art pieces
        {
            try
            {
                var connectionString = AppConfig.Value.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var deleteArtQuery = "DELETE art where art.id = @ArtId";
                    using (SqlCommand deleteArtCommand = new SqlCommand(deleteArtQuery, connection))
                    {
                        deleteArtCommand.Parameters.AddWithValue("@ArtId", ArtId);
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
        public async Task<IEnumerable<Artist>> GetArtistsByArtId(int id)
        {
            var contributingArtists = new Artist();
            var artists = new List<Artist>();
            var connectionString = AppConfig.Value.ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                var query1 =
                    @"
                    Select ContributingArtists.ArtistId, Artist.Name from ContributingArtists
                    left join Artist on ContributingArtists.ArtistId = Artist.Id where ContributingArtists.ArtId = @ArtId; ";
                using (var command = new SqlCommand(query1, connection))
                {
                    command.Parameters.AddWithValue("@ArtId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            contributingArtists = new Artist()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            };
                            artists.Add(contributingArtists);
                        }
                        return artists.ToArray();
                    }
                }
            }
        }
        public async void DeleteContributingArtist(int ArtId,int ArtistId)
        {
            try
            {
                var connectionString = AppConfig.Value.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var deleteConArtQuery = "DELETE ContributingArtists where ArtId = @ArtId and ArtistId = @ArtistId;";
                    using (SqlCommand deleteConArtCommand = new SqlCommand(deleteConArtQuery, connection))
                    {
                        deleteConArtCommand.Parameters.AddWithValue("@ArtId", ArtId);
                        deleteConArtCommand.Parameters.AddWithValue("@ArtistId", ArtistId);
                        await deleteConArtCommand.ExecuteNonQueryAsync();
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error in DeleteContributingArtist");
                throw;
            }
        }
    }
}

