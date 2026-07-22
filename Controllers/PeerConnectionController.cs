using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Career_Guidance_Platform.Hubs;
using Career_Guidance_Platform.Service;

namespace Career_Guidance_Platform.Controllers
{
    [Authorize]
    public class PeerConnectionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<PresenceAndNotificationHub> _hubContext;
        private readonly PresenceTracker _tracker;

        public PeerConnectionController(
            AppDbContext context, 
            UserManager<User> userManager,
            IHubContext<PresenceAndNotificationHub> hubContext,
            PresenceTracker tracker)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _tracker = tracker;
        }

        // GET: /PeerConnection/Index (Connections manager page)
        public async Task<IActionResult> Index()
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue))
                return Challenge();

            var userId = int.Parse(userIdValue);

            // Fetch accepted friends
            var friends = await _context.PeerConnections
                .Where(c => c.Status == "Accepted" && (c.RequesterId == userId || c.ReceiverId == userId))
                .Include(c => c.Requester)
                .Include(c => c.Receiver)
                .Select(c => c.RequesterId == userId ? c.Receiver : c.Requester)
                .ToListAsync();

            // Fetch pending requests received from others
            var pendingReceived = await _context.PeerConnections
                .Where(c => c.Status == "Pending" && c.ReceiverId == userId)
                .Include(c => c.Requester)
                .ToListAsync();

            // Fetch pending requests sent to others
            var pendingSent = await _context.PeerConnections
                .Where(c => c.Status == "Pending" && c.RequesterId == userId)
                .Include(c => c.Receiver)
                .ToListAsync();

            ViewBag.PendingReceived = pendingReceived;
            ViewBag.PendingSent = pendingSent;
            ViewBag.CurrentUserId = userId;

            return View("~/Views/Profile/Connections.cshtml", friends);
        }

        // POST: /PeerConnection/SendRequest
        [HttpPost]
        public async Task<IActionResult> SendRequest(int receiverId)
        {
            var senderIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(senderIdValue))
                return Json(new { success = false, message = "Chưa đăng nhập." });

            var senderId = int.Parse(senderIdValue);
            if (senderId == receiverId)
                return Json(new { success = false, message = "Không thể tự kết nối với bản thân." });

            var senderUser = await _userManager.FindByIdAsync(senderIdValue);
            var receiverUser = await _context.Users.FindAsync(receiverId);
            if (senderUser == null || receiverUser == null)
                return Json(new { success = false, message = "Người dùng không tồn tại." });

            // Check if connection already exists
            var existing = await _context.PeerConnections
                .FirstOrDefaultAsync(c =>
                    (c.RequesterId == senderId && c.ReceiverId == receiverId) ||
                    (c.RequesterId == receiverId && c.ReceiverId == senderId));

            if (existing != null)
            {
                if (existing.Status == "Accepted")
                    return Json(new { success = false, message = "Các bạn đã là bạn bè." });
                if (existing.Status == "Pending")
                    return Json(new { success = false, message = "Yêu cầu kết nối đang được xử lý." });
                if (existing.Status == "Blocked")
                    return Json(new { success = false, message = "Không thể gửi yêu cầu kết nối." });
            }

            // Limit Free account: check requests sent today
            if (!senderUser.IsPremium)
            {
                var startOfToday = DateTime.Today;
                var sentToday = await _context.PeerConnections
                    .CountAsync(c => c.RequesterId == senderId && c.SentAt >= startOfToday);

                if (sentToday >= 5)
                {
                    return Json(new { success = false, message = "Bạn đã đạt giới hạn 5 lượt kết nối/ngày của tài khoản Free. Nâng cấp Premium để kết nối không giới hạn!" });
                }
            }

            // Create connection request
            var connection = new PeerConnection
            {
                RequesterId = senderId,
                ReceiverId = receiverId,
                Status = "Pending",
                SentAt = DateTime.Now
            };
            _context.PeerConnections.Add(connection);

            // Create notification
            var notification = new Notification
            {
                UserId = receiverId,
                Message = $"{senderUser.FullName} đã gửi cho bạn một lời mời kết nối bạn bè.",
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            // Real-time notify via SignalR
            var notificationCount = await _context.Notifications.CountAsync(n => n.UserId == receiverId && !n.IsRead);
            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveNotification", $"{senderUser.FullName} đã gửi cho bạn một lời mời kết nối bạn bè.", notificationCount);
            
            // Push request to Receiver's connections manager panel dynamically
            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveConnectionRequest", new
            {
                requesterId = senderId,
                requesterName = senderUser.FullName,
                avatarUrl = senderUser.AvatarUrl,
                headline = senderUser.Headline
            });

            return Json(new { success = true, message = "Đã gửi lời mời kết nối thành công!" });
        }

        // POST: /PeerConnection/AcceptRequest
        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requesterId)
        {
            var receiverIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(receiverIdValue))
                return Json(new { success = false, message = "Chưa đăng nhập." });

            var receiverId = int.Parse(receiverIdValue);
            var receiverUser = await _userManager.FindByIdAsync(receiverIdValue);

            var connection = await _context.PeerConnections
                .Include(c => c.Requester)
                .FirstOrDefaultAsync(c => c.RequesterId == requesterId && c.ReceiverId == receiverId && c.Status == "Pending");

            if (connection == null)
                return Json(new { success = false, message = "Không tìm thấy yêu cầu kết nối." });

            connection.Status = "Accepted";
            connection.ConnectedAt = DateTime.Now;

            // Notify requester
            var notification = new Notification
            {
                UserId = requesterId,
                Message = $"{receiverUser?.FullName} đã chấp nhận lời mời kết nối của bạn.",
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            // Real-time notify via SignalR
            var notificationCount = await _context.Notifications.CountAsync(n => n.UserId == requesterId && !n.IsRead);
            await _hubContext.Clients.User(requesterId.ToString()).SendAsync("ReceiveNotification", $"{receiverUser?.FullName} đã chấp nhận lời mời kết nối của bạn.", notificationCount);

            // Fetch online status of both
            var receiverOnline = await _tracker.IsUserOnline(receiverId);
            var requesterOnline = await _tracker.IsUserOnline(requesterId);

            // Broadcast accepted status to update lists dynamically
            await _hubContext.Clients.User(requesterId.ToString()).SendAsync("ConnectionAccepted", new
            {
                friendId = receiverId,
                friendName = receiverUser?.FullName,
                avatarUrl = receiverUser?.AvatarUrl,
                headline = receiverUser?.Headline,
                isOnline = receiverOnline
            });

            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ConnectionAccepted", new
            {
                friendId = requesterId,
                friendName = connection.Requester?.FullName,
                avatarUrl = connection.Requester?.AvatarUrl,
                headline = connection.Requester?.Headline,
                isOnline = requesterOnline
            });

            return Json(new { success = true, message = "Đã chấp nhận kết nối thành công!" });
        }

        // POST: /PeerConnection/DeclineRequest
        [HttpPost]
        public async Task<IActionResult> DeclineRequest(int requesterId)
        {
            var receiverIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(receiverIdValue))
                return Json(new { success = false, message = "Chưa đăng nhập." });

            var receiverId = int.Parse(receiverIdValue);

            var connection = await _context.PeerConnections
                .FirstOrDefaultAsync(c => c.RequesterId == requesterId && c.ReceiverId == receiverId && c.Status == "Pending");

            if (connection == null)
                return Json(new { success = false, message = "Không tìm thấy yêu cầu kết nối." });

            _context.PeerConnections.Remove(connection);
            await _context.SaveChangesAsync();

            // Real-time notify both clients to update UI lists
            await _hubContext.Clients.User(requesterId.ToString()).SendAsync("ConnectionRemoved", receiverId);
            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ConnectionRemoved", requesterId);

            return Json(new { success = true, message = "Đã từ chối kết nối thành công." });
        }

        // POST: /PeerConnection/Disconnect
        [HttpPost]
        public async Task<IActionResult> Disconnect(int targetId)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue))
                return Json(new { success = false, message = "Chưa đăng nhập." });

            var userId = int.Parse(userIdValue);

            var connection = await _context.PeerConnections
                .FirstOrDefaultAsync(c =>
                    (c.RequesterId == userId && c.ReceiverId == targetId && c.Status == "Accepted") ||
                    (c.RequesterId == targetId && c.ReceiverId == userId && c.Status == "Accepted"));

            if (connection == null)
                return Json(new { success = false, message = "Không tồn tại mối quan hệ kết nối." });

            _context.PeerConnections.Remove(connection);
            await _context.SaveChangesAsync();

            // Real-time notify both clients to update UI lists
            await _hubContext.Clients.User(targetId.ToString()).SendAsync("ConnectionRemoved", userId);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ConnectionRemoved", targetId);

            return Json(new { success = true, message = "Đã hủy kết nối bạn bè thành công." });
        }
    }
}
