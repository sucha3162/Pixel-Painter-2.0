using Microsoft.Data.SqlClient;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Interfaces
{
    /// <summary>
    /// Interface defines the SQL service.
    /// </summary>
    public interface ILikeService
    {
        /// <summary>
        /// Inserts a like entry into the database.
        /// </summary>
        /// <returns>Returns the number of rows changed, or -1</returns>
        public Task<int> InsertLike(int artId, Artist artist);
        /// <summary>
        /// Removes a specific like entry into the database.
        /// </summary>
        /// <returns>Returns the number of rows changed, or -1</returns>
        public Task<int> RemoveLike(int artId, Artist artist);

        /// <summary>
        /// Checks whether the currently logged in user has liked a particular art piece.
        /// </summary>
        /// <returns>Returns true if yes and false if no.</returns>
        public Task<bool> IsLiked(int artId, Artist artist);
        /// <summary>
        /// Gets all the likes of an Artwork
        /// </summary>
        /// <param name="artworkId">Artwork Id to get likes from</param>
        /// <returns></returns>
        public IEnumerable<Like> GetLikesByArtwork(int artworkId);

        public Task<Like> GetLikeByIds(int artId, int artistId);

    }
}