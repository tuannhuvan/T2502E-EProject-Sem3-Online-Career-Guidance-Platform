using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Career_Guidance_Platform.Service
{
    public class PresenceTracker
    {
        // Maps UserId -> Set of active ConnectionIds
        private static readonly ConcurrentDictionary<int, HashSet<string>> OnlineUsers = 
            new ConcurrentDictionary<int, HashSet<string>>();

        public Task<bool> UserConnected(int userId, string connectionId)
        {
            bool isFirstConnection = false;

            OnlineUsers.AddOrUpdate(userId,
                // Add new entry
                (id) => {
                    isFirstConnection = true;
                    return new HashSet<string> { connectionId };
                },
                // Update existing entry
                (id, connections) => {
                    lock (connections)
                    {
                        if (connections.Count == 0)
                        {
                            isFirstConnection = true;
                        }
                        connections.Add(connectionId);
                    }
                    return connections;
                }
            );

            return Task.FromResult(isFirstConnection);
        }

        public Task<bool> UserDisconnected(int userId, string connectionId)
        {
            bool isCompletelyOffline = false;

            if (OnlineUsers.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        isCompletelyOffline = true;
                        OnlineUsers.TryRemove(userId, out _);
                    }
                }
            }

            return Task.FromResult(isCompletelyOffline);
        }

        public Task<int[]> GetOnlineUsers()
        {
            int[] onlineUserIds;
            lock (OnlineUsers)
            {
                onlineUserIds = OnlineUsers.Keys.ToArray();
            }
            return Task.FromResult(onlineUserIds);
        }

        public Task<List<string>> GetConnectionsForUser(int userId)
        {
            List<string> connectionIds = new List<string>();
            if (OnlineUsers.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connectionIds = connections.ToList();
                }
            }
            return Task.FromResult(connectionIds);
        }

        public Task<bool> IsUserOnline(int userId)
        {
            return Task.FromResult(OnlineUsers.ContainsKey(userId));
        }
    }
}
