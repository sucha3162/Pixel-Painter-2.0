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
                    using (var reader = command.ExecuteReader())
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
                            { //Art Table + NumLikes and NumComments
                                id = reader.GetInt32(0),
                                title = reader.GetString(1),
                                creationDate = reader.GetDateTime(5),
                                isPublic = reader.GetBoolean(6),
                                numLikes = reader.GetInt32(7),
                                numComments = reader.GetInt32(8),
                                pixelGrid = pixelGrid,
                            };
                            painting.SetArtists(GetArtists(painting.id));
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
                    WHERE Art.ID =  {id} 
                    GROUP BY Art.ID, Art.Title, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic;
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
                                width = reader.GetInt32(2),
                                height = reader.GetInt32(3),
                                encodedGrid = reader.GetString(4)
                            };
                            painting = new Art
                            { //ArtId, ArtName, Width, ArtLength, Encode, Date, IsPublic
                                id = reader.GetInt32(0),
                                title = reader.GetString(1),
                                pixelGrid = pixelGrid,
                                creationDate = reader.GetDateTime(5),
                                isPublic = reader.GetBoolean(6),
                                numLikes = reader.GetInt32(7),
                                numComments = reader.GetInt32(8)
                            };
                            painting.SetArtists(GetArtists(painting.id));
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
                      left join  ContributingArtists as CA on CA.ArtId = Art.Id
                      WHERE CA.ArtistId = @ArtistID
                    ";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ArtistID", artistId);
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

        public async Task<Art> SaveNewArt(Artist artist, Art art) //Single Artist
        {
            try
            {
                art.creationDate = DateTime.UtcNow;

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
                            command.Parameters.AddWithValue("@Title", art.title);
                            command.Parameters.AddWithValue("@ArtistId", artist.id);
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

        public async Task<Art> SaveNewArtMulti(Art art)//Multi artist
        {
            try
            {
                art.creationDate = DateTime.UtcNow;

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
                        command.Parameters.AddWithValue("@Title", art.title);
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

        public async Task AddContributingArtist(int artId, int artistId)
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
                        command.ExecuteNonQuery();

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
                var oldArt = GetArtById(art.id);
                if (oldArt == null)
                {
                    return null; // old art does not exist, thus cant be deleted
                }
                //else if (oldArt.artistId != artist.id)
                //{
                //    throw new UnauthorizedAccessException("User does not have permission to update this art");
                //}
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

        public async Task DeleteArt(int ArtId) //change to admin only and have it so users can remove themselves from art pieces
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
        public Artist[] GetArtists(int id)
        {
            var ContributingArtists = new Artist();
            var Artists = new List<Artist>();
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
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ContributingArtists = new Artist()
                            {
                                id = reader.GetInt32(0),
                                name = reader.GetString(1)
                            };
                            Artists.Add(ContributingArtists);
                        }
                        return Artists.ToArray();
                    }
                }
            }
        }
        public async Task DeleteContributingArtist(int ArtId,int ArtistId)
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
                        deleteConArtCommand.ExecuteNonQuery();
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

