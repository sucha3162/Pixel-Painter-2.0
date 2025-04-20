using Microsoft.Data.SqlClient;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Interfaces
{
    /// <summary>
    /// Interface defines the SQL service.
    /// </summary>
    public interface ILikeService
    {
        public Task<int> InsertLike(int artId, Artist artist);
        public Task<int> RemoveLike(int artId, Artist artist);
        public Task<bool> IsLiked(int artId, Artist artist);
        public Task<IEnumerable<Like>> GetLikesByArtwork(int artworkId);
        public Task<Like> GetLikeByIds(int artId, int artistId);

    }
}