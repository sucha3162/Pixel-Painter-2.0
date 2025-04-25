using MyTestVueApp.Server.Entities;
using System.Numerics;

namespace MyTestVueApp.Server.Interfaces
{
    public interface IConnectionManager
    {
        public void AddGroup(string groupName, string[][][] canvas, int canvasSize, string backgroundColor);
        public void RemoveGroup(string groupName);
        public void AddUser(string connectionId, Artist member, string groupName);
        public void RemoveUserFromGroup(string connectionId, Artist artist, string groupName);
        public void RemoveUserFromAllGroups(string connectionId);
        public void PaintPixels(string groupName, int layer, string color, Coordinate[] vector);
        public Group GetGroup(string groupName);
        public IEnumerable<Group> GetGroups();
        public IEnumerable<GroupAdvert> GetGroupAdverts();
        public IEnumerable<Artist> GetUsersInGroup(string groupName);
        public IEnumerable<Artist> GetContributingArtists(string groupName);
        public bool GroupExists(string groupName);
        public bool HasConnection(string connectionId);
    }
}
