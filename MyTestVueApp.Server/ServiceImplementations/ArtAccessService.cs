using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using Microsoft.Data.SqlClient;

namespace MyTestVueApp.Server.ServiceImplementations
{
    public class ArtAccessService : IArtAccessService
    {
        private IOptions<ApplicationConfiguration> AppConfig { get; }
        private ILogger<ArtAccessService> Logger { get; }
        private ILoginService LoginService { get; }
        public ArtAccessService(IOptions<ApplicationConfiguration> appConfig, ILogger<ArtAccessService> logger, ILoginService loginService)
        {
            AppConfig = appConfig;
            Logger = logger;
            LoginService = loginService;
        }

        public IEnumerable<Art> GetAllArt()
        {
            var paintings = new List<Art>();
            var connectionString = AppConfig.Value.ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //var query = "SELECT Date, TemperatureC, Summary FROM WeatherForecasts";
                var query =
                    @"
                    Select 
	                    Art.Id, 
	                    Art.Title, 
	                    Art.ArtistId, 
	                    Artist.Name, 
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
	                    LEFT JOIN Artist ON Art.ArtistId = Artist.Id
                    GROUP BY Art.ID, Art.Title, Art.ArtistID, Artist.Name, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic;
                    ";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pixelGrid = new PixelGrid()
                            {
                                width = reader.GetInt32(4),
                                height = reader.GetInt32(5),
                                encodedGrid = reader.GetString(6)
                            };
                            var painting = new Art
                            { //Art Table + NumLikes and NumComments
                                id = reader.GetInt32(0),
                                title = reader.GetString(1),
                                artistId = reader.GetInt32(2),
                                artistName = reader.GetString(3),
                                creationDate = reader.GetDateTime(7),
                                isPublic = reader.GetBoolean(8),
                                numLikes = reader.GetInt32(9),
                                numComments = reader.GetInt32(10),
                                pixelGrid = pixelGrid,
                            };
                            paintings.Add(painting);
                        }
                    }
                }
            }
            return paintings;
        }

        public Art GetArtById(int id)
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
	                    Art.Artistid, 
	                    Artist.Name,
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
                    LEFT JOIN Artist ON Art.ArtistId = Artist.Id
                    WHERE Art.ID =  {id} 
                    GROUP BY Art.ID, Art.Title, Art.ArtistID, Artist.Name, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic;
                    ";

                //SQL INJECTION WHOOPS^
                //Good thing we have type checking :p

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            var pixelGrid = new PixelGrid()
                            {
                                width = reader.GetInt32(4),
                                height = reader.GetInt32(5),
                                encodedGrid = reader.GetString(6)
                            };
                            painting = new Art
                            { //ArtId, ArtName, ArtistId, Width, ArtLength, Encode, Date, IsPublic
                                id = reader.GetInt32(0),
                                title = reader.GetString(1),
                                artistId = reader.GetInt32(2),
                                artistName = reader[3] as string ?? string.Empty,
                                pixelGrid = pixelGrid,
                                creationDate = reader.GetDateTime(7),
                                isPublic = reader.GetBoolean(8),
                                numLikes = reader.GetInt32(9),
                                numComments = reader.GetInt32(10)
                            };
                            return painting;
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<Art> GetArtByArtist(int artistId)
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
	                    Art.Title
                    FROM ART
                    WHERE Art.ID =  {artistId}
                    ";
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var painting = new Art
                            {   //ArtId, ArtName
                                id = reader.GetInt32(0),
                                title = reader.GetString(1)
                            };
                            paintings.Add(painting);
                        }
                    }
                }
                return paintings;
            }
        }

        public async Task<Art> SaveNewArt(Artist artist, Art art)
        {
            try
            {
                art.artistId = artist.id;
                art.creationDate = DateTime.UtcNow;

                using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                {
                    connection.Open();

                    var query = @"
                    INSERT INTO Art (Title, ArtistId, Width, Height, Encode, CreationDate, IsPublic)
                    VALUES (@Title, @ArtistId, @Width, @Height, @Encode, @CreationDate, @IsPublic);
                    SELECT SCOPE_IDENTITY();
                ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Title", art.title);
                        command.Parameters.AddWithValue("@ArtistId", art.artistId);
                        command.Parameters.AddWithValue("@Width", art.pixelGrid.width);
                        command.Parameters.AddWithValue("@Height", art.pixelGrid.height);
                        command.Parameters.AddWithValue("@Encode", art.pixelGrid.encodedGrid);
                        command.Parameters.AddWithValue("@CreationDate", art.creationDate);
                        command.Parameters.AddWithValue("@IsPublic", art.isPublic);

                        var newArtId = await command.ExecuteScalarAsync();
                        art.id = Convert.ToInt32(newArtId);
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

        public async Task<Art> UpdateArt(Artist artist, Art art)
        {
            try
            {
                var oldArt = GetArtById(art.id);
                if (oldArt == null)
                {
                    return null; // old art does not exist, thus cant be deleted
                }
                else if (oldArt.artistId != artist.id)
                {
                    throw new UnauthorizedAccessException("User does not have permission to update this art");
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
                            WHERE Id = @Id AND ArtistID = @ArtistID;
                        ";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Title", art.title);
                            command.Parameters.AddWithValue("@IsPublic", art.isPublic);
                            command.Parameters.AddWithValue("@Width", art.pixelGrid.width);
                            command.Parameters.AddWithValue("@Height", art.pixelGrid.height);
                            command.Parameters.AddWithValue("@Encode", art.pixelGrid.encodedGrid);
                            command.Parameters.AddWithValue("@Id", art.id);
                            command.Parameters.AddWithValue("@ArtistId", artist.id);

                             await command.ExecuteScalarAsync();
                            
                            return GetArtById(Convert.ToInt32(art.id));
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

        public async Task DeleteArt(int ArtId)
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

