using Microsoft.Data.SqlClient;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Interfaces
{
    /// <summary>
    /// Interface defines the SQL service.
    /// </summary>
    public interface IArtAccessService
    {

        /// <summary>
        /// Gets a list of all paintings from the database.
        /// </summary>
        /// <returns>A list of all paintings</returns>
        public IEnumerable<Art> GetAllArt();
        public Art GetArtById(int id);
        public IEnumerable<Art> GetArtByArtist(int artistId);
        public Artist[] GetArtists(int artId);
        public Task DeleteArt(int artId);
        public Task DeleteContributingArtist(int artid,int artistid);
        public Task<Art> SaveNewArt(Artist artist, Art art);
        public Task<Art> SaveNewArtMulti(Art art);
        public Task AddContributingArtist(int artId, int artistId);
        public Task<Art> UpdateArt(Artist artist, Art art);
        public Task<IEnumerable<Art>> SaveGif(Artist artist, Art[] art);

        public Task<IEnumerable<Art>> GetLikedArt(int artistId);
        public Task<IEnumerable<Art>> GetGif(int id);
        public Task<Art> UpdateGif(Art[] art, int fps);
    }
}