using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.ServiceImplementations
{
    public class ConnectionManager : IConnectionManager
    {
        // groupName, Group
        Dictionary<string, Group> Groups = new();
        // ConnectionID, Artist
        Dictionary<string, Artist> ArtistLookup = new();
        //artistId, MembershipRecord
        Dictionary<int, MembershipRecord> Records = new();

        public void AddGroup(string groupName, List<Artist> contributors, string[][][] canvas, int canvasSize, string backgroundColor)
        {
            Groups.Add(groupName, new Group(groupName, contributors, canvas, canvasSize, backgroundColor));
        }
        public void AddUser(string connectionId, Artist artist, string groupName)
        {
            if (!Groups.ContainsKey(groupName))
            {
                throw new ArgumentException("This group does not exist, so we can not add a user to it!");
            }
         
            ArtistLookup.Add(connectionId, artist);
            Groups[groupName].AddMember(artist);
            if (Records.ContainsKey(artist.Id))
            {
                Records[artist.Id].Connections.Add(new(connectionId, groupName));
            } else
            {
                Records.Add(artist.Id, new(connectionId, artist.Id, groupName));
            }
        }
        public void RemoveUserFromGroup(string connectionId, Artist artist, string groupName)
        {

            if (!Records.ContainsKey(artist.Id))
            {
                throw new ArgumentException("This artist is not tracked by the connection manager, so we cant remove them!");
            }

            MembershipRecord record = Records[artist.Id];
            List<ConnectionBinding> allConnectionsToGroup = new();
            ConnectionBinding? connectionToDelete = null;

            foreach(ConnectionBinding binding in record.Connections)
            {
                if (binding.groupName == groupName)
                {
                    allConnectionsToGroup.Add(binding);
                    if (binding.connectionId == connectionId)
                    {
                        connectionToDelete = binding;
                    }
                }    
            }

            if (connectionToDelete == null || allConnectionsToGroup.Count == 0) 
            { // Invalid request
                throw new ArgumentException("RemoveUserFromGroup: Invalid ConnectionId!");
            }
            
            if (allConnectionsToGroup.Count() == 1)
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
                Records.Remove(artist.Id);
            }
          
            if (Groups[groupName].IsEmpty())
            { // Delete group if it is empty
                Groups.Remove(groupName);
            }
        }

        public void RemoveUserFromAllGroups(string connectionId)
        {

            if (!ArtistLookup.ContainsKey(connectionId) || !Records.ContainsKey(ArtistLookup[connectionId].Id) )
            {
                throw new ArgumentException("RemoveUserFromGroup: This connection doesnt exist, so we cannot remove it!");
            }

            Artist artist = ArtistLookup[connectionId];
            MembershipRecord record = Records[artist.Id];
            HashSet<string> groups = new();
            foreach (ConnectionBinding cb in record.Connections) {
                if (cb.connectionId == connectionId)
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

        public bool HasConnection(string connectionId)
        {
            return ArtistLookup.ContainsKey(connectionId);
        }
    }
}
