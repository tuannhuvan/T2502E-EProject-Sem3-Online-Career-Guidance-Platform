using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Service;
using System.Collections.Generic;

namespace Career_Guidance_Platform.Hubs
{
    [Authorize]
    public class PresenceAndNotificationHub : Hub
    {
        private readonly PresenceTracker _tracker;
        private readonly AppDbContext _context;

        public PresenceAndNotificationHub(PresenceTracker tracker, AppDbContext context)
        {
            _tracker = tracker;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                bool isOnline = await _tracker.UserConnected(userId, Context.ConnectionId);

                // Fetch user's friends (Accepted PeerConnections)
                var friendIds = await _context.PeerConnections
                    .Where(c => c.Status == "Accepted" && (c.RequesterId == userId || c.ReceiverId == userId))
                    .Select(c => c.RequesterId == userId ? c.ReceiverId : c.RequesterId)
                    .ToListAsync();

                // If this is the user's first active tab/connection, notify online friends
                if (isOnline)
                {
                    foreach (var friendId in friendIds)
                    {
                        if (await _tracker.IsUserOnline(friendId))
                        {
                            await Clients.User(friendId.ToString()).SendAsync("UserWentOnline", userId);
                        }
                    }
                }

                // Send current list of online friends to the caller
                var onlineFriends = new List<int>();
                foreach (var friendId in friendIds)
                {
                    if (await _tracker.IsUserOnline(friendId))
                    {
                        onlineFriends.Add(friendId);
                    }
                }

                await Clients.Caller.SendAsync("OnlineFriendsList", onlineFriends);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdStr = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                bool isOffline = await _tracker.UserDisconnected(userId, Context.ConnectionId);

                // If user is completely offline, notify online friends
                if (isOffline)
                {
                    var friendIds = await _context.PeerConnections
                        .Where(c => c.Status == "Accepted" && (c.RequesterId == userId || c.ReceiverId == userId))
                        .Select(c => c.RequesterId == userId ? c.ReceiverId : c.RequesterId)
                        .ToListAsync();

                    foreach (var friendId in friendIds)
                    {
                        if (await _tracker.IsUserOnline(friendId))
                        {
                            await Clients.User(friendId.ToString()).SendAsync("UserWentOffline", userId);
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
