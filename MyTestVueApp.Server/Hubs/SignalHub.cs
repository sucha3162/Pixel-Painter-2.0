using Microsoft.AspNetCore.SignalR;
using System.Drawing;


namespace MyTestVueApp.Server.Hubs
{

    public class Vector2
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    public class SignalHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }

        public async Task SendPixel(string room, string color, Vector2 coord)
        {
            Console.WriteLine("Room:" + room + "Color: " + color + "Coord: " + coord.ToString());
            await Clients.OthersInGroup(room).SendAsync("ReceivePixel", color, coord);
        }

        public async Task SendPixels(string room, string color, Vector2[] coords)
        {
            Console.WriteLine("Room:" + room + "Color: " + color + "Coord: " + coords.ToString());
            await Clients.OthersInGroup(room).SendAsync("ReceivePixels", color, coords);
        }

        public async Task SendBucket(string room, string color, Vector2 coord)
        {
            await Clients.OthersInGroup(room).SendAsync("ReceiveBucket", color, coord);
        }

        public async Task SendMessage(string user, string room, string message)
        {
            await Clients.Group(room).SendAsync(user, message);
        }
    }
}
