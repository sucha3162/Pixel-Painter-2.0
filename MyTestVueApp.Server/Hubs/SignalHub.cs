
using Microsoft.AspNetCore.SignalR;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.Entities;


namespace MyTestVueApp.Server.Hubs
{
    public class SignalHub : Hub
    {
        //key = groupname, value = list of users in the group
        private IConnectionManager Manager;
        private readonly ILogger<SignalHub> Logger;

        private Dictionary<string, string> Users = new();

        public SignalHub(IConnectionManager manager, ILogger<SignalHub> logger)
        {
            Manager = manager;
            Logger = logger;
        }

        public async Task CreateOrJoinGroup(string groupName, Artist artist, string[][] canvas, int canvasSize, string backgroundColor)
        {
            Logger.LogInformation("GroupName: " + groupName + " GroupExists: " + Manager.GroupExists(groupName));
            if (Manager.GroupExists(groupName))
            {
                Logger.LogInformation("Joining Group!");
                await JoinGroup(groupName, artist);
            } else
            {
                Logger.LogInformation("Creating Group!");
                await CreateGroup(groupName, artist, canvas, canvasSize, backgroundColor);
            }
        }

        public async Task JoinGroup(string groupName, Artist artist)
        {
            // Add the user to the group
            Manager.AddUser(groupName, artist); 
            Users.Add(Context.ConnectionId, groupName);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{artist.Name} has joined the group {groupName}.");

            Logger.LogInformation("Member Count: " + Manager.GetGroup(groupName).GetCurrentMemberCount());
           
            // Send the group config and list of members to the joiner!
            await Clients.Client(Context.ConnectionId).SendAsync("GroupConfig", Manager.GetGroup(groupName).CanvasSize, Manager.GetGroup(groupName).BackgroundColor, Manager.GetGroup(groupName).GetPixelsAsList());
            await Clients.Client(Context.ConnectionId).SendAsync("Members", Manager.GetGroup(groupName).CurrentMembers);
            
            // Tell Existing members about the new member!
            await Clients.Group(groupName).SendAsync("NewMember", artist);

            // Log Members to console
            string members = "";
            foreach(Artist member in Manager.GetGroup(groupName).CurrentMembers)
            {
                members = members + " " + member.Name;
            }
            Logger.LogInformation("Members: " + members);
        }


        public async Task CreateGroup(string groupName, Artist artist, string[][] canvas, int canvasSize, string backgroundColor)
        { 
            // Create the group, then add the user
            Manager.AddGroup(groupName,canvas,canvasSize,backgroundColor);
            Manager.AddUser(groupName, artist);
            Users.Add(Context.ConnectionId, groupName);

            Logger.LogInformation("Group Info: " 
                + Manager.GetGroup(groupName).Name + " " 
                + Manager.GetGroup(groupName).CanvasSize + " "
                + Manager.GetGroup(groupName).BackgroundColor
                );


            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{artist.Name} has joined the group {groupName}.");
            // Give self to the frontend............ yeah i know.
            await Clients.Group(groupName).SendAsync("NewMember", artist);

        }

        public async Task LeaveGroup(string groupName, Artist member)
        {
            Logger.LogInformation("Leaving Group - MC: " + Manager.GetGroup(groupName).GetCurrentMemberCount());
            Manager.RemoveUser(groupName, member);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{member.Name} has left the group {groupName}.");
        }

        public async Task SendPixel(string room, string color, Coordinate coord)
        {
            Manager.PaintPixels(room, color, [coord]);
            await Clients.Group(room).SendAsync("ReceivePixel", color, coord);
        }

        public async Task SendPixels(string room, string color, Coordinate[] coords)
        {
            Manager.PaintPixels(room, color, coords);
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
            Manager.GetGroup(groupName).BackgroundColor = backgroundColor;
            await Clients.Group(groupName).SendAsync("BackgroundColor", backgroundColor);
        }

        public async Task GetGroupMembers(string groupName)
        {
            await Clients.Group(groupName).SendAsync("GroupMembers", Manager.GetGroup(groupName).CurrentMembers);
        }

        public async Task GetContributingArtists(string groupName)
        {
            await Clients.Group(groupName).SendAsync("ContributingArtists", Manager.GetGroup(groupName).MemberRecord);
        }
    }
}
