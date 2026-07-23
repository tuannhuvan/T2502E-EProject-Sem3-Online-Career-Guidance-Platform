using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;

namespace Career_Guidance_Platform.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProfileController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Profile/Details/{userId}
        public async Task<IActionResult> Details(int id)
        {
            var targetUser = await _context.Users
                .Include(u => u.MentorProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (targetUser == null)
            {
                return NotFound("Không tìm thấy người dùng này.");
            }

            var currentUserIdStr = _userManager.GetUserId(User);
            bool isSelf = false;
            string connectionStatus = "None"; // None, PendingSent, PendingReceived, Accepted, Blocked
            
            if (!string.IsNullOrEmpty(currentUserIdStr))
            {
                var currentUserId = int.Parse(currentUserIdStr);
                isSelf = currentUserId == id;

                if (!isSelf)
                {
                    // Check connection status
                    var connection = await _context.PeerConnections
                        .FirstOrDefaultAsync(c => 
                            (c.RequesterId == currentUserId && c.ReceiverId == id) ||
                            (c.RequesterId == id && c.ReceiverId == currentUserId));

                    if (connection != null)
                    {
                        if (connection.Status == "Blocked")
                        {
                            connectionStatus = "Blocked";
                        }
                        else if (connection.Status == "Accepted")
                        {
                            connectionStatus = "Accepted";
                        }
                        else if (connection.Status == "Pending")
                        {
                            if (connection.RequesterId == currentUserId)
                                connectionStatus = "PendingSent";
                            else
                                connectionStatus = "PendingReceived";
                        }
                    }
                }
            }

            // Get user's completed courses
            var completedCourses = await _context.UserCourseProgresses
                .Where(ucp => ucp.UserId == id && ucp.Status == "Completed")
                .Include(ucp => ucp.Course)
                .Select(ucp => ucp.Course!.Title)
                .ToListAsync();

            // Get user's active/latest resume template code if they have one
            var latestResume = await _context.Resumes
                .Where(r => r.UserId == id)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .FirstOrDefaultAsync();

            // Get recommended path from test
            var latestResult = await _context.TestResults
                .Where(tr => tr.UserId == id && tr.RecommendedCareerPathId.HasValue)
                .Include(tr => tr.RecommendedCareerPath)
                .OrderByDescending(tr => tr.CreatedAt)
                .FirstOrDefaultAsync();

            ViewBag.IsSelf = isSelf;
            ViewBag.ConnectionStatus = connectionStatus;
            ViewBag.CompletedCourses = completedCourses;
            ViewBag.LatestResume = latestResume;
            ViewBag.RecommendedPath = latestResult?.RecommendedCareerPath?.Title;

            return View(targetUser);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string headline, string school, string major, string experience, string avatarUrl)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue))
                return Json(new { success = false, message = "Chưa đăng nhập." });

            var user = await _userManager.FindByIdAsync(userIdValue);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng." });

            user.Headline = headline;
            user.School = school;
            user.Major = major;
            user.Experience = experience;
            user.AvatarUrl = avatarUrl;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Lỗi cập nhật thông tin." });
        }
    }
}
