using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.ServiceImplementations
{
    public class ConnectionManager : IConnectionManager
    {
        // groupName, Group
        Dictionary<string, Group> Groups = new();
        Dictionary<string, Artist> ArtistLookup = new();
        Dictionary<int, MembershipRecord> Records = new();

        public void AddGroup(string groupName, string[][][] canvas, int canvasSize, string backgroundColor)
        {
            Groups.Add(groupName, new Group(groupName, canvas, canvasSize, backgroundColor));
        }

        public void AddUser(string connectionId, Artist artist, string groupName)
        {
            Console.WriteLine("AddUser: (" + connectionId + ", " + artist.id + ", " + groupName); // Presentation
            if (Groups.ContainsKey(groupName))
            {
                ArtistLookup.Add(connectionId, artist);
                Groups[groupName].AddMember(artist);
                if (Records.ContainsKey(artist.id))
                {
                    Console.WriteLine("User already exists, so adding to connections!"); // Presentation
                    Records[artist.id].Connections.Add(new(connectionId, groupName));
                } else
                {
                    Console.WriteLine("New User!"); // Presentation
                    Records.Add(artist.id, new(connectionId, artist.id, groupName));
                }
            }
            Console.WriteLine(Records.Count());

        }

        public void RemoveUserFromGroup(string connectionId, Artist artist, string groupName)
        {
            Console.WriteLine("#Users before removal: " + Groups[groupName].CurrentMembers.Count); //Presentation

            if (!Records.ContainsKey(artist.id))
            {
                throw new ArgumentException("This artist is not tracked by the connection manager, so we cant remove them!");
            }

            MembershipRecord record = Records[artist.id];
            List<ConnectionBinding> allUserConnections = new();
            ConnectionBinding? connectionToDelete = null;

            foreach(ConnectionBinding binding in record.Connections)
            {

                if (binding.groupName == groupName)
                {
                    allUserConnections.Add(binding);
                    if (binding.connectionId == connectionId)
                    {
                        connectionToDelete = binding;
                    }
                }    
            }

            if (connectionToDelete == null || allUserConnections.Count == 0) 
            { // Invalid request
                throw new ArgumentException("RemoveUserFromGroup: Invalid ConnectionId!");
            }
            
            if (allUserConnections.Count() == 1)
            { // Remove member from group
                Groups[groupName].RemoveMember(artist);
                record.Connections.Remove(connectionToDelete);
                ArtistLookup.Remove(connectionId);
            }else
            { // Just remove the connection
                record.Connections.Remove(connectionToDelete);
                ArtistLookup.Remove(connectionId);
            }

            if (record.Connections.Count == 0)
            { // Remove record from records if the Artist doesnt have any open connections;
                Console.WriteLine("Artist has zero connections. Removing from List!"); // Presentation
                Records.Remove(artist.id);
            }
          
            Console.WriteLine("Remaining Users: " +  Groups[groupName].CurrentMembers.Count); // Presentation
            if (Groups[groupName].IsEmpty())
            { // Delete group if it is empty
                Console.WriteLine("Deleting Group"); // Presentation
                Groups.Remove(groupName);
            }
        }

        public void RemoveUserFromAllGroups(string connectionId)
        {

            if (!ArtistLookup.ContainsKey(connectionId) || !Records.ContainsKey(ArtistLookup[connectionId].id) )
            {
                throw new ArgumentException("RemoveUserFromGroup: This connection doesnt exist, so we cannot remove it!");
            }

            Artist artist = ArtistLookup[connectionId];
            MembershipRecord record = Records[artist.id];
            HashSet<string> groups = new();
            foreach (ConnectionBinding cb in record.Connections) {
                groups.Add(cb.groupName);
            }

            foreach (string groupName in groups)
            {
                try
                {
                    RemoveUserFromGroup(connectionId, artist, groupName);
                } catch 
                {
                    throw;
                }
            }
        }

        public IEnumerable<Group> GetGroups()
        {
            return Groups.Values;
        }

        public IEnumerable<GroupAdvert> GetGroupAdverts()
        {
            List<GroupAdvert> groupAdverts = new();
            foreach (Group group in  Groups.Values)
            {
                groupAdverts.Add(new GroupAdvert(group.Name, group.CurrentMembers.Count));
            }
            return groupAdverts;
        }

        public IEnumerable<Artist> GetUsersInGroup(string groupName)
        {
            return Groups[groupName].CurrentMembers;
        }

        public IEnumerable<Artist> GetContributingArtists(string groupName)
        {
            return Groups[groupName].MemberRecord;
        }

        public void PaintPixels(string groupName, int layer, string color, Coordinate[] vector)
        {
            Groups[groupName].PaintPixels(layer, color, vector);
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
            return Groups.ContainsKey(groupName);
        }
    }
}
