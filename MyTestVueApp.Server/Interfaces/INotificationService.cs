using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Interfaces
{
    public interface INotificationService
    {
        public IEnumerable<Notification> GetNotificationsForArtist(int artistId);
        public Task<bool> MarkComment(int commentId);
        public Task<bool> MarkLike(int artId, int artistId);


    }
}
