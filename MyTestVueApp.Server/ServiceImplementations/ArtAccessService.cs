using Microsoft.Extensions.Options;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Entities;
using MyTestVueApp.Server.Interfaces;
using Microsoft.Data.SqlClient;
using System.Linq;
using ImageMagick;

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
        /// <summary>
        /// Gets all artworks from the database
        /// </summary>
        /// <returns>A List of Art objects</returns>
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
                                IsGif = reader.GetBoolean(7),
                                NumLikes = reader.GetInt32(8),
                                NumComments = reader.GetInt32(9),
                                GifID = reader.GetInt32(10),
                                GifFrameNum = reader.GetInt32(11),
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
        /// <summary>
        /// Gets an artwork by it's Id
        /// </summary>
        /// <param name="id">Id of the artwork to be retrieved</param>
        /// <returns>An art object</returns>
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
                        Art.IsGIF,
                        Art.GifId,
	                    COUNT(distinct Likes.ArtistId) as Likes, 
	                    Count(distinct Comment.Id) as Comments  
                    FROM ART  
                    LEFT JOIN Likes ON Art.ID = Likes.ArtID  
                    LEFT JOIN Comment ON Art.ID = Comment.ArtID  
                    LEFT JOIN ContributingArtists ON Art.Id = ContributingArtists.ArtId
                    WHERE Art.ID = @artId 
                    GROUP BY Art.ID, Art.Title, Art.Width, Art.Height, Art.Encode, Art.CreationDate, Art.isPublic, Art.IsGIF, Art.gifId;
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
                                NumLikes = reader.GetInt32(9),
                                NumComments = reader.GetInt32(10),
                                IsGif = reader.GetBoolean(7),
                                GifID = reader.GetInt32(8)
                            };
                            painting.SetArtists((await GetArtistsByArtId(painting.Id)).ToList());
                            return painting;
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Grabs all artwork than and artist made
        /// </summary>
        /// <param name="artistId">Id of the artist</param>
        /// <returns>A list of artworks</returns>
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
                                Width = reader.GetInt32(2),
                                Height = reader.GetInt32(3),
                                EncodedGrid = reader.GetString(4)
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
        /// <summary>
        /// Gets all art that a user has liked
        /// </summary>
        /// <param name="artistId">Id of the user</param>
        /// <returns>A list of art objects</returns>
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
                                Width = reader.GetInt32(2),
                                Height = reader.GetInt32(3),
                                EncodedGrid = reader.GetString(4)
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
        /// <summary>
        /// Adds a new artwork to the database
        /// </summary>
        /// <param name="artist">User who is creating the art</param>
        /// <param name="art">Artwork being added to the database</param>
        /// <returns>The id of the art created</returns>
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
                        command.Parameters.AddWithValue("@Width", art.PixelGrid.Width);
                        command.Parameters.AddWithValue("@Height", art.PixelGrid.Height);
                        command.Parameters.AddWithValue("@Encode", art.PixelGrid.EncodedGrid);
                        command.Parameters.AddWithValue("@CreationDate", art.CreationDate);
                        command.Parameters.AddWithValue("@IsPublic", art.IsPublic);
                        command.Parameters.AddWithValue("@ArtistId", artist.Id);

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
                                command.Parameters.AddWithValue("@ArtistId", artist.Id);
                                command.Parameters.AddWithValue("@Width", item.PixelGrid.Width);
                                command.Parameters.AddWithValue("@Height", item.PixelGrid.Height);
                                command.Parameters.AddWithValue("@Encode", item.PixelGrid.EncodedGrid);
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
                                command.Parameters.AddWithValue("@ArtistId", artist.Id);
                                command.Parameters.AddWithValue("@Width", item.PixelGrid.Width);
                                command.Parameters.AddWithValue("@Height", item.PixelGrid.Height);
                                command.Parameters.AddWithValue("@Encode", item.PixelGrid.EncodedGrid);
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
                                Width = reader.GetInt32(4),
                                Height = reader.GetInt32(5),
                                EncodedGrid = reader.GetString(6)
                            };
                            var gifFrames = new Art()
                            {
                                Id = reader.GetInt32(0),
                                GifID = reader.GetInt32(2),
                                GifFrameNum = reader.GetInt32(3),
                                PixelGrid = pixelGrid,
                                GifFps = await GetGifFps(id)

                            };
                            gifFrames.SetArtists((await GetArtistsByArtId(gifFrames.Id)).ToList());
                            paintings.Add(gifFrames);
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
        /// <summary>
        /// Adds an artist to the database with the artwork they helped create
        /// </summary>
        /// <param name="artId">Art Id that is being reference</param>
        /// <param name="artistId">Artist's Id who is being added</param>
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
        /// <summary>
        /// Updates an artwork in the database
        /// </summary>
        /// <param name="art">Updated version of the artwork</param>
        /// <returns>The updated artwork object</returns>
        public async Task<Art> UpdateArt(Art art)
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
                                command.Parameters.AddWithValue("@Width", item.PixelGrid.Width);
                                command.Parameters.AddWithValue("@Height", item.PixelGrid.Height);
                                command.Parameters.AddWithValue("@Encode", item.PixelGrid.EncodedGrid);
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
                    return await GetArtById(Convert.ToInt32(art[0].Id));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in UpdateGif");
                throw;
            }

        }
        /// <summary>
        /// Removes an artwork from the database
        /// </summary>
        /// <param name="ArtId">Id of the artwork to be removed</param>
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
        /// <summary>
        /// Gets a list of artists who worked on an artwork
        /// </summary>
        /// <param name="id">Id of the artwork being referenced</param>
        /// <returns>A list of Artists</returns>
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
        /// <summary>
        /// Removes an artist from a list of users who worked on an artwork
        /// </summary>
        /// <param name="ArtId">Id of the art that the user is being removed from</param>
        /// <param name="ArtistId">Id of the artist being removed</param>
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

