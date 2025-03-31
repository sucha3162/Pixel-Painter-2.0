using MyTestVueApp.Server.Entities;
using System.Numerics;

namespace MyTestVueApp.Server.Interfaces
{
    public interface IConnectionManager
    {
        public void AddGroup(string groupName);
        public void AddGroup(string groupName, string[][] canvas, int canvasSize, string backgroundColor);
        public void RemoveGroup(string groupName);
        public void AddUser(string groupName, Artist member);
        public void RemoveUser(string groupName, Artist member);
        public void PaintPixels(string groupName, string color, Coordinate[] vector);
        public Group GetGroup(string groupName);
        public IEnumerable<string> GetGroups();
        public IEnumerable<Artist> GetUsersInGroup(string groupName);
        public IEnumerable<Artist> GetContributingArtists(string groupName);
        public bool GroupExists(string groupName);
    }
}
