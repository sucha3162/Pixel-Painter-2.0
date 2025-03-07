using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Interfaces
{
    public interface INotificationService
    {
        public IEnumerable<Notification> GetNotificationsForArtist(int artistId);
    }
}
