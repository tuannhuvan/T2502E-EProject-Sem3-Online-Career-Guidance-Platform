using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Career_Guidance_Platform.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ChatController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Chat?peerId=5
        public async Task<IActionResult> Index(int? peerId)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue))
                return Challenge();

            var userId = int.Parse(userIdValue);

            // 1. Get friends (Accepted PeerConnections)
            var friendConnections = await _context.PeerConnections
                .Where(c => c.Status == "Accepted" && (c.RequesterId == userId || c.ReceiverId == userId))
                .Include(c => c.Requester)
                .Include(c => c.Receiver)
                .ToListAsync();

            var allowedUserIds = new HashSet<int>();
            foreach (var conn in friendConnections)
            {
                allowedUserIds.Add(conn.RequesterId == userId ? conn.ReceiverId : conn.RequesterId);
            }

            // 2. Get mentors/mentees with completed meetings
            var completedMeetings = await _context.MentorshipMeetings
                .Where(m => m.Status == "Completed" && (m.MenteeId == userId || m.MentorId == userId))
                .ToListAsync();

            foreach (var meet in completedMeetings)
            {
                allowedUserIds.Add(meet.MenteeId == userId ? meet.MentorId : meet.MenteeId);
            }

            // Load profiles of all allowed chat partners
            var chatPartners = await _context.Users
                .Where(u => allowedUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName, u.AvatarUrl, u.Headline })
                .ToListAsync();

            ViewBag.ChatPartners = chatPartners;
            ViewBag.CurrentUserId = userId;

            User? activePartner = null;
            var chatHistory = new List<MentorshipMessage>();

            if (peerId.HasValue)
            {
                // Verify access permission
                if (!allowedUserIds.Contains(peerId.Value))
                {
                    TempData["ChatError"] = "Bạn chỉ có thể nhắn tin với bạn bè đã kết nối hoặc Cố vấn / Học viên đã hoàn thành buổi làm việc.";
                    return RedirectToAction(nameof(Index));
                }

                activePartner = await _context.Users.FindAsync(peerId.Value);
                if (activePartner != null)
                {
                    chatHistory = await _context.MentorshipMessages
                        .Where(m =>
                            (m.SenderId == userId && m.ReceiverId == peerId.Value) ||
                            (m.SenderId == peerId.Value && m.ReceiverId == userId))
                        .OrderBy(m => m.CreatedAt)
                        .ToListAsync();

                    // Mark messages from partner to current user as Read
                    var unread = await _context.MentorshipMessages
                        .Where(m => m.SenderId == peerId.Value && m.ReceiverId == userId && !m.IsRead)
                        .ToListAsync();

                    if (unread.Any())
                    {
                        foreach (var msg in unread)
                        {
                            msg.IsRead = true;
                        }
                        await _context.SaveChangesAsync();
                    }
                }
            }

            ViewBag.ActivePartner = activePartner;
            return View(chatHistory);
        }
    }
}
