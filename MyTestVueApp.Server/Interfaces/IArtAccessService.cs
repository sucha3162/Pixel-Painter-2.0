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
        // public IEnumerable<Art> GetArtByLikes(bool order); // Gets art sorted by likes either in ascending or descending order
        // public IEnumerable<Art> GetArtByComments(bool order);
       // public IEnumerable<Art> GetArtByDate(bool order);
        public Art GetArtById(int id);
        public IEnumerable<Art> GetArtByArtist(int artistId);

        public Artist[] GetArtists(int artId);
        public Task DeleteArt(int artId);
        public Task DeleteContributingArtist(int artid,int artistid);
        public Task<Art> SaveNewArt(Artist artist, Art art);
        public Task<Art> UpdateArt(Artist artist, Art art);
    }
}