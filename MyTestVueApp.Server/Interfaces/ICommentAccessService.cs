using Microsoft.Data.SqlClient;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Interfaces
{
    /// <summary>
    /// Interface defines the SQL service.
    /// </summary>
    public interface ICommentAccessService
    {
        public Task<int> DeleteComment(int commentId);
        public Task<int> EditComment(int commentId, string newMessage);
        public Task<IEnumerable<Comment>> GetCommentsByArtId(int id);
        public Task<Comment> CreateComment(Artist commenter, Comment comment);
        public Task<Comment> GetCommentByCommentId(int CommentId);
        public Task<IEnumerable<Comment>> GetReplyByCommentId(int id);
        public Task<IEnumerable<Comment>> GetCommentByUserId(int id);
    }
}