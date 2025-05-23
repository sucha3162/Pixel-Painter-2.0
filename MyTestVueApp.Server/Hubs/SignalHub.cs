﻿
using Microsoft.AspNetCore.SignalR;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.Entities;

namespace MyTestVueApp.Server.Hubs
{
    public class SignalHub : Hub
    {
        private readonly IConnectionManager Manager;
        private readonly ILogger<SignalHub> Logger;

        public SignalHub(IConnectionManager manager, ILogger<SignalHub> logger)
        {
            Manager = manager;
            Logger = logger;
        }

        public async Task JoinGroup(string groupName, Artist artist)
        {
            if (!Manager.GroupExists(groupName))
            {
                Logger.LogError("User attempted to join a group that doesnt exist!");
                throw new HubException("User attempted to join a group that doesnt exist!");
            }

            Manager.AddUser(Context.ConnectionId, artist, groupName); 
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{artist.Name} has joined the group {groupName}.");
           
            await Clients.Client(Context.ConnectionId).SendAsync("GroupConfig", Manager.GetGroup(groupName).CanvasSize, Manager.GetGroup(groupName).BackgroundColor, Manager.GetGroup(groupName).GetPixelsAsList());
            await Clients.Client(Context.ConnectionId).SendAsync("Members", Manager.GetGroup(groupName).MemberRecord);
            
            await Clients.Group(groupName).SendAsync("NewMember", artist);
        }


        public async Task CreateGroup(string groupName, Artist artist, List<Artist> contributors, string[][][] canvas, int canvasSize, string backgroundColor)
        {
            if (Manager.GroupExists(groupName))
            {
                Logger.LogError("User attempted to create a group that already exists!");
                throw new HubException("User attempted to create a group that already exists!");
            }

            Manager.AddGroup(groupName, contributors, canvas,canvasSize,backgroundColor);
            Manager.AddUser(Context.ConnectionId, artist, groupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{artist.Name} has joined the group {groupName}.");
        }

        public async Task LeaveGroup(string groupName, Artist member)
        {
            try
            {
                Manager.RemoveUserFromGroup(Context.ConnectionId, member, groupName);
            } catch (ArgumentException ex)
            {
                Logger.LogError(ex.Message);
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{member.Name} has left the group {groupName}.");
        }

        public async Task SendPixels(string room, int layer, string color, Coordinate[] coords)
        {
            Manager.PaintPixels(room, layer, color, coords);
            await Clients.Group(room).SendAsync("ReceivePixels", layer, color, coords);
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

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Manager.HasConnection(Context.ConnectionId))
            {
                try
                {
                    Manager.RemoveUserFromAllGroups(Context.ConnectionId);
                }
                catch (ArgumentException ex)
                {
                    Logger.LogError(ex.Message);
                }
            }

            if (exception != null)
            {
                Logger.LogError($"Error, Disconnected: {exception.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
