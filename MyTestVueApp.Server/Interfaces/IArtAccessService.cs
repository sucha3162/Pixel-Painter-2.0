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
        public Task<IEnumerable<Art>> GetAllArt();
        public Task<Art> GetArtById(int id);
        public Task<IEnumerable<Art>> GetArtByArtist(int artistId);
        public Task<IEnumerable<Artist>> GetArtistsByArtId(int artId);
        public void DeleteArt(int artId);
        public void DeleteContributingArtist(int artid,int artistid);
        public Task<Art> SaveNewArt(Artist artist, Art art);
        public Task<Art> SaveNewArtMulti(Art art);
        public void AddContributingArtist(int artId, int artistId);
        public Task<Art> UpdateArt(Artist artist, Art art);
        public Task<IEnumerable<Art>> GetLikedArt(int artistId);
    }
}