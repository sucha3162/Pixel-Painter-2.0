using Microsoft.AspNetCore.Connections.Features;

namespace MyTestVueApp.Server.Entities
{
    public record ConnectionBinding (string connectionId, string groupName);
    public class MembershipRecord(string connectionId, int artistId, string group)
    {
        public int ArtistId { get; set; } = artistId;
        public List<ConnectionBinding> Connections { get; set; } = new List<ConnectionBinding> { new(connectionId,group) };
    }
}
