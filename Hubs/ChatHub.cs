using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;

namespace Career_Guidance_Platform.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string receiverIdStr, string message)
        {
            var senderIdStr = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderIdStr) || string.IsNullOrEmpty(receiverIdStr) || string.IsNullOrWhiteSpace(message))
                return;

            int senderId = int.Parse(senderIdStr);
            int receiverId = int.Parse(receiverIdStr);

            // Save message to mentorship_messages
            var chatMsg = new MentorshipMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.MentorshipMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            // Send to receiver and sender in real-time
            await Clients.User(receiverIdStr).SendAsync("ReceiveMessage", senderIdStr, message, chatMsg.CreatedAt.ToString("dd/MM/yyyy HH:mm"), chatMsg.Id);
            await Clients.User(senderIdStr).SendAsync("ReceiveMessage", senderIdStr, message, chatMsg.CreatedAt.ToString("dd/MM/yyyy HH:mm"), chatMsg.Id);
        }
    }
}
