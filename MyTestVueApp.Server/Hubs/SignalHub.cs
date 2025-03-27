
using Microsoft.AspNetCore.SignalR;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.Entities;


namespace MyTestVueApp.Server.Hubs
{
    public class SignalHub : Hub
    {
        //key = groupname, value = list of users in the group
        private IConnectionManager _manager;
        private readonly ILogger<SignalHub> _logger;

        private Dictionary<string, string> users = new();

        public SignalHub(IConnectionManager manager, ILogger<SignalHub> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        public async Task CreateOrJoinGroup(string groupName, Artist artist, string[][] canvas, int canvasSize, string backgroundColor)
        {
            _logger.LogInformation("GroupName: " + groupName + " GroupExists: " + _manager.GroupExists(groupName));
            if (_manager.GroupExists(groupName))
            {
                _logger.LogInformation("Joining Group!");
                await JoinGroup(groupName, artist);
            } else
            {
                _logger.LogInformation("Creating Group!");
                await CreateGroup(groupName, artist, canvas, canvasSize, backgroundColor);
            }
        }

        public async Task JoinGroup(string groupName, Artist artist)
        {
            // Add the user to the group
            _manager.AddUser(groupName, artist); 
            users.Add(Context.ConnectionId, groupName);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{artist.name} has joined the group {groupName}.");

            _logger.LogInformation("Member Count: " + _manager.GetGroup(groupName).GetCurrentMemberCount());
           
            // Send the group config and list of members to the joiner!
            await Clients.Client(Context.ConnectionId).SendAsync("GroupConfig", _manager.GetGroup(groupName).CanvasSize, _manager.GetGroup(groupName).BackgroundColor, _manager.GetGroup(groupName).GetPixelsAsList());
            await Clients.Client(Context.ConnectionId).SendAsync("Members", _manager.GetGroup(groupName).CurrentMembers);
            
            // Tell Existing members about the new member!
            await Clients.Group(groupName).SendAsync("NewMember", artist);

            // Log Members to console
            string members = "";
            foreach(Artist member in _manager.GetGroup(groupName).CurrentMembers)
            {
                members = members + " " + member.name;
            }
            _logger.LogInformation("Members: " + members);
        }


        public async Task CreateGroup(string groupName, Artist artist, string[][] canvas, int canvasSize, string backgroundColor)
        { 
            // Create the group, then add the user
            _manager.AddGroup(groupName,canvas,canvasSize,backgroundColor);
            _manager.AddUser(groupName, artist);
            users.Add(Context.ConnectionId, groupName);

            _logger.LogInformation("Group Info: " 
                + _manager.GetGroup(groupName).Name + " " 
                + _manager.GetGroup(groupName).CanvasSize + " "
                + _manager.GetGroup(groupName).BackgroundColor
                );


            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{artist.name} has joined the group {groupName}.");
            // Give self to the frontend............ yeah i know.
            await Clients.Group(groupName).SendAsync("NewMember", artist);

        }

        public async Task LeaveGroup(string groupName, Artist member)
        {
            _logger.LogInformation("Leaving Group - MC: " + _manager.GetGroup(groupName).GetCurrentMemberCount());
            _manager.RemoveUser(groupName, member);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{member.name} has left the group {groupName}.");
        }

        public async Task SendPixel(string room, string color, Coordinate coord)
        {
            _manager.PaintPixels(room, color, [coord]);
            await Clients.Group(room).SendAsync("ReceivePixel", color, coord);
        }

        public async Task SendPixels(string room, string color, Coordinate[] coords)
        {
            _manager.PaintPixels(room, color, coords);
            await Clients.Group(room).SendAsync("ReceivePixels", color, coords);
        }

        public async Task SendBucket(string room, string color, Coordinate coord)
        { // WARNING Not sending all painted pixels!
            await Clients.Group(room).SendAsync("ReceiveBucket", color, coord);
        }

        public async Task SendMessage(string user, string room, string message)
        {
            await Clients.Group(room).SendAsync(user, message);
        }

        public async Task ChangeBackgroundColor(string groupName, string backgroundColor)
        {
            _manager.GetGroup(groupName).BackgroundColor = backgroundColor;
            await Clients.Group(groupName).SendAsync("BackgroundColor", backgroundColor);
        }

        public async Task GetGroupMembers(string groupName)
        {
            await Clients.Group(groupName).SendAsync("GroupMembers", _manager.GetGroup(groupName).CurrentMembers);
        }

        public async Task GetContributingArtists(string groupName)
        {
            await Clients.Group(groupName).SendAsync("ContributingArtists", _manager.GetGroup(groupName).MemberRecord);
        }
    }
}
