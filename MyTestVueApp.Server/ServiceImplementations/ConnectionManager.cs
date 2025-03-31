using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.ServiceImplementations
{
    public class ConnectionManager : IConnectionManager
    {
        Dictionary<string, Group> Groups = new();
        public void AddGroup(string groupName)
        {
            Groups.Add(groupName, new Group(groupName));
        }

        public void AddGroup(string groupName, string[][] canvas, int canvasSize, string backgroundColor)
        {
            Groups.Add(groupName, new Group(groupName, canvas, canvasSize, backgroundColor));
        }

        public void AddUser(string groupName, Artist member)
        { // Add a user to group 
            if (Groups.ContainsKey(groupName))
            { // Just add User
                Groups[groupName].AddMember(member);

            } else
            {
                Groups.Add(groupName, new Group(groupName));
                Groups[groupName].AddMember(member);
            }
        }

        public void RemoveUser(string groupName, Artist member)
        {
            Groups[groupName].RemoveMember(member);
            if (Groups[groupName].IsEmpty())
            {
                Groups.Remove(groupName);
            }
        }

        public IEnumerable<string> GetGroups()
        {
            return Groups.Keys;
        }

        public IEnumerable<Artist> GetUsersInGroup(string groupName)
        {
            return Groups[groupName].CurrentMembers;
        }

        public IEnumerable<Artist> GetContributingArtists(string groupName)
        {
            return Groups[groupName].MemberRecord;
        }

        public void PaintPixels(string groupName, string color, Coordinate[] vector)
        {
            Groups[groupName].PaintPixels(color, vector);
        }

        public void RemoveGroup(string groupName)
        {
            Groups.Remove(groupName);
        }

        public Group GetGroup(string groupName)
        {
            return Groups[groupName];
        }

        public bool GroupExists(string groupName)
        {
            Console.WriteLine("GroupExists!");
            return Groups.ContainsKey(groupName);
        }
    }
}
