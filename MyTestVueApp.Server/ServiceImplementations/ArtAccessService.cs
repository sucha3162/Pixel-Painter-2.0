using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using Microsoft.Data.SqlClient;
using ImageMagick;

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
						Art.IsGIF,
	                    COUNT(distinct Likes.ArtistId) as Likes, 
	                    Count(distinct Comment.Id) as Comments,
                        Art.gifId,
                        Art.gifFrameNum
                    FROM ART  
	                    LEFT JOIN Likes ON Art.ID = Likes.ArtID  
	                    LEFT JOIN Comment ON Art.ID = Comment.ArtID
						Where Art.gifFrameNum <= 1
                    GROUP BY Art.ID, Art.Title, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic, Art.IsGIF, Art.GifId, Art.gifFrameNum
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
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                CreationDate = reader.GetDateTime(5),
                                IsPublic = reader.GetBoolean(6),
                                IsGif = reader.GetBoolean(7),
                                NumLikes = reader.GetInt32(8),
                                NumComments = reader.GetInt32(9),
                                GifID = reader.GetInt32(10),
                                GifFrameNum = reader.GetInt32(11),
                                PixelGrid = pixelGrid,
                            };
                            painting.SetArtists(GetArtists(painting.Id));
                            paintings.Add(painting);
                        }
                    }
                }
            }
            return paintings;
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

        //Pull all art related to user
        //Pulls art by Id
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
                        Art.IsGif,
                        Art.GifId,
	                    COUNT(distinct Likes.ArtistId) as Likes, 
	                    Count(distinct Comment.Id) as Comments  
                    FROM ART  
                    LEFT JOIN Likes ON Art.ID = Likes.ArtID  
                    LEFT JOIN Comment ON Art.ID = Comment.ArtID  
                    WHERE Art.ID =  {id} 
                    GROUP BY Art.ID, Art.Title, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic, Art.isGif,Art.GifId;
                    ";

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
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                PixelGrid = pixelGrid,
                                CreationDate = reader.GetDateTime(5),
                                IsPublic = reader.GetBoolean(6),
                                NumLikes = reader.GetInt32(9),
                                NumComments = reader.GetInt32(10),
                                IsGif = reader.GetBoolean(7),
                                GifID = reader.GetInt32(8)
                            };
                            painting.SetArtists(GetArtists(painting.Id));
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
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                PixelGrid = pixelGrid,
                                CreationDate = reader.GetDateTime(5),
                                IsPublic = reader.GetBoolean(6),
                                NumLikes = reader.GetInt32(7),
                                NumComments = reader.GetInt32(8)
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
                        command.Parameters.AddWithValue("@Width", art.PixelGrid.width);
                        command.Parameters.AddWithValue("@Height", art.PixelGrid.height);
                        command.Parameters.AddWithValue("@Encode", art.PixelGrid.encodedGrid);
                        command.Parameters.AddWithValue("@CreationDate", art.CreationDate);
                        command.Parameters.AddWithValue("@IsPublic", art.IsPublic);
                        command.Parameters.AddWithValue("@ArtistId", artist.id);

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

        public async Task<IEnumerable<Art>> SaveGif(Artist artist, Art[] art)
        {

            try
            {
                using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                {
                    connection.Open();

                    var query = @"
                    INSERT INTO Gif(FPS) values (@FPS);
                    SELECT SCOPE_IDENTITY();
                ";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FPS", art[0].GifFps);
                        var GifID = await command.ExecuteScalarAsync();
                        foreach (var item in art)
                        {
                            if (GifID != null)
                            item.GifID = Convert.ToInt32(GifID);
                        }

                    }
                }
                foreach (var item in art)
                {
                    item.CreationDate = DateTime.UtcNow;
                    if(item.GifFrameNum == 1) {
                        using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                        {
                            connection.Open();

                            var query = @"
                    INSERT INTO Art (Title, Width, Height, Encode, CreationDate, IsPublic, IsGIF, gifId,gifFrameNum)
                    VALUES (@Title, @Width, @Height, @Encode, @CreationDate, @IsPublic,1,@gifNum,@frameNum);
                    SELECT SCOPE_IDENTITY();
                    INSERT INTO ContributingArtists(ArtId,ArtistId) values (@@IDENTITY,@ArtistId);";
                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Title", item.Title);
                                command.Parameters.AddWithValue("@ArtistId", artist.id);
                                command.Parameters.AddWithValue("@Width", item.PixelGrid.width);
                                command.Parameters.AddWithValue("@Height", item.PixelGrid.height);
                                command.Parameters.AddWithValue("@Encode", item.PixelGrid.encodedGrid);
                                command.Parameters.AddWithValue("@CreationDate", item.CreationDate);
                                command.Parameters.AddWithValue("@IsPublic", item.IsPublic);
                                command.Parameters.AddWithValue("@gifNum", item.GifID);
                                command.Parameters.AddWithValue("@frameNum", item.GifFrameNum);

                                var newArtId = await command.ExecuteScalarAsync();
                                item.Id = Convert.ToInt32(newArtId);
                            }
                        }

                    }
                    else {
                        using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                        {
                            connection.Open();

                            var query = @"
                    INSERT INTO Art (Title, Width, Height, Encode, CreationDate, IsPublic, IsGIF, gifId,gifFrameNum)
                    VALUES (@Title, @Width, @Height, @Encode, @CreationDate, @IsPublic,1,@gifNum,@frameNum);
                    SELECT SCOPE_IDENTITY();
                    ;";
                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Title", item.Title);
                                command.Parameters.AddWithValue("@ArtistId", artist.id);
                                command.Parameters.AddWithValue("@Width", item.PixelGrid.width);
                                command.Parameters.AddWithValue("@Height", item.PixelGrid.height);
                                command.Parameters.AddWithValue("@Encode", item.PixelGrid.encodedGrid);
                                command.Parameters.AddWithValue("@CreationDate", item.CreationDate);
                                command.Parameters.AddWithValue("@IsPublic", item.IsPublic);
                                command.Parameters.AddWithValue("@gifNum", item.GifID);
                                command.Parameters.AddWithValue("@frameNum", item.GifFrameNum);

                                var newArtId = await command.ExecuteScalarAsync();
                                item.Id = Convert.ToInt32(newArtId);
                            }
                        }
                    }
                }
                return  art;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in SaveArt");
                throw;
            }
        }

        public async Task<IEnumerable<Art>> GetGif(int id)
        {
            var connectionString = AppConfig.Value.ConnectionString;
            var paintings = new List<Art>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query =
                    @"
                        Select id,Title,gifId,gifFrameNum,Width,Height,Encode from art
                        where gifId = @gifId
                        Order by gifFrameNum";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@gifId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var pixelGrid = new PixelGrid()
                            {
                                width = reader.GetInt32(4),
                                height = reader.GetInt32(5),
                                encodedGrid = reader.GetString(6)
                            };
                            var Gifframes = new Art()
                            {
                                Id = reader.GetInt32(0),
                                GifID = reader.GetInt32(2),
                                GifFrameNum = reader.GetInt32(3),
                                PixelGrid = pixelGrid,
                                GifFps = await GetGifFps(id)

                            };
                            Gifframes.SetArtists(GetArtists(Gifframes.Id));
                            paintings.Add(Gifframes);
                        }
                        return paintings;
                    }
                }
            }
        }

        public async Task<int> GetGifFps(int id)
        {
            var connectionString = AppConfig.Value.ConnectionString;
            var fps = 0;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query =
                    @"
                        Select FPS from GIF
                        where Id = @gifId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@gifId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            fps = reader.GetInt32(0);
                        }
                        return fps;
                    }
                }
            }
        }
        public async Task<Art> SaveNewArtMulti(Art art)
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
                        command.Parameters.AddWithValue("@Width", art.PixelGrid.width);
                        command.Parameters.AddWithValue("@Height", art.PixelGrid.height);
                        command.Parameters.AddWithValue("@Encode", art.PixelGrid.encodedGrid);
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
                            command.Parameters.AddWithValue("@Width", art.PixelGrid.width);
                            command.Parameters.AddWithValue("@Height", art.PixelGrid.height);
                            command.Parameters.AddWithValue("@Encode", art.PixelGrid.encodedGrid);
                            command.Parameters.AddWithValue("@Id", art.Id);

                            await command.ExecuteScalarAsync();

                            return GetArtById(Convert.ToInt32(art.Id));
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

        public async Task<Art> UpdateGif(Art[] art, int Fps)
        {
            try
            {
                var gifNum = art[0].GifID;
                var oldArt = GetGif(gifNum);
                if (oldArt == null)
                {
                    return null;
                }

                else
                {
                    foreach (var item in art)
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
                            WHERE GifId = @Id;
                        ";
                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Title", item.Title);
                                command.Parameters.AddWithValue("@IsPublic", item.IsPublic);
                                command.Parameters.AddWithValue("@Width", item.PixelGrid.width);
                                command.Parameters.AddWithValue("@Height", item.PixelGrid.height);
                                command.Parameters.AddWithValue("@Encode", item.PixelGrid.encodedGrid);
                                command.Parameters.AddWithValue("@Id",gifNum);

                                await command.ExecuteScalarAsync();

                            }
                        }
                    }
                    using (var connection = new SqlConnection(AppConfig.Value.ConnectionString))
                    {
                        connection.Open();

                        var query = @"
                           Update GIF Set
                           FPS = @Fps
                           where Id = @GifID
                        ";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Fps", Fps);
                            command.Parameters.AddWithValue("@GifID", gifNum);

                            await command.ExecuteScalarAsync();

                        }
                    }
                        return GetArtById(Convert.ToInt32(art[0].Id));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in UpdateGif");
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
        public async Task DeleteContributingArtist(int ArtId, int ArtistId)
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