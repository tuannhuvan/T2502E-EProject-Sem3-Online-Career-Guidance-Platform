using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Data;
using OfficeOpenXml;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using Career_Guidance_Platform.Hubs;

namespace Career_Guidance_Platform.Areas.Mentor.Controllers
{
    [Area("Mentor")]
    [Authorize(Roles = "Mentor")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public DashboardController(AppDbContext context, UserManager<User> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        private const int EarlyCompleteWindowMinutes = 30;
        private const int ExpireGraceHours = 3;

        private async Task AutoExpireOverdueMeetingsAsync(int? mentorId = null, int? menteeId = null)
        {
            var cutoff = DateTime.Now.AddHours(-ExpireGraceHours);

            var query = _context.MentorshipMeetings
                .Where(mm => mm.Status == "Scheduled" && mm.ScheduledTime < cutoff);

            if (mentorId.HasValue) query = query.Where(mm => mm.MentorId == mentorId.Value);
            if (menteeId.HasValue) query = query.Where(mm => mm.MenteeId == menteeId.Value);

            var overdueMeetings = await query.ToListAsync();
            if (overdueMeetings.Count == 0) return;

            foreach (var meeting in overdueMeetings)
            {
                meeting.Status = "Expired";
            }

            await _context.SaveChangesAsync();
        }

        // GET: /Mentor/Dashboard
        public async Task<IActionResult> Index()
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var mentorProfile = await _context.MentorProfiles.FirstOrDefaultAsync(m => m.UserId == userId);
            if (mentorProfile == null)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Quét và tự động đánh dấu "Expired" các lịch hẹn quá hạn trước khi hiển thị
            await AutoExpireOverdueMeetingsAsync(mentorId: userId);

            // Lấy các yêu cầu kết nối đang chờ duyệt
            var requests = await _context.MentorshipRequests
                .Include(r => r.Mentee)
                .Where(r => r.MentorId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Lấy danh sách lịch hẹn
            var meetings = await _context.MentorshipMeetings
                .Include(mm => mm.Mentee)
                .Where(mm => mm.MentorId == userId)
                .OrderBy(mm => mm.Status == "Completed" ? 1 : 0)
                .ThenBy(mm => mm.ScheduledTime)
                .ToListAsync();

            // Lấy danh sách đánh giá từ người học
            var reviews = await _context.MentorReviews
                .Include(r => r.Mentee)
                .Where(r => r.MentorId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.Meetings = meetings;
            ViewBag.Reviews = reviews;

            return View(mentorProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleRequest(int requestId, string status)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Json(new { success = false, message = "Bạn chưa đăng nhập." });
            var userId = int.Parse(userIdValue);

            var request = await _context.MentorshipRequests.FindAsync(requestId);
            if (request == null || request.MentorId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu kết nối." });
            }

            var allowedStatuses = new[] { "Approved", "Rejected", "Cancelled" };
            if (!allowedStatuses.Contains(status))
            {
                return Json(new { success = false, message = "Trạng thái không hợp lệ." });
            }

            if (request.Status != "Pending")
            {
                return Json(new { success = false, message = "Yêu cầu này đã được xử lý trước đó, không thể thay đổi lại." });
            }

            request.Status = status;
            await _context.SaveChangesAsync();

            // Create notification for Mentee (User)
            var mentorUser = await _userManager.GetUserAsync(User);
            var mentorName = mentorUser?.FullName ?? "Cố vấn";
            var statusVietnamese = status == "Approved" ? "phê duyệt" : (status == "Rejected" ? "từ chối" : status);
            var msg = $"Cố vấn {mentorName} đã {statusVietnamese} yêu cầu kết nối của bạn.";

            var notification = new Notification
            {
                UserId = request.MenteeId,
                Message = msg,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Push SignalR notification to Mentee
            try
            {
                await _hubContext.Clients.User(request.MenteeId.ToString()).SendAsync("ReceiveNotification", msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] SignalR failed to push notification to user {request.MenteeId}: {ex.Message}");
            }

            string statusText = status == "Approved" ? "Phê duyệt" : (status == "Rejected" ? "Từ chối" : status);
            return Json(new { success = true, message = $"Đã cập nhật trạng thái yêu cầu kết nối thành công: {statusText}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteMeeting(int meetingId)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Json(new { success = false, message = "Bạn chưa đăng nhập." });
            var userId = int.Parse(userIdValue);

            var meeting = await _context.MentorshipMeetings.FindAsync(meetingId);
            if (meeting == null || meeting.MentorId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy buổi tư vấn." });
            }

            if (meeting.Status == "Expired" || meeting.Status == "NoShow")
            {
                return Json(new { success = false, message = "Buổi tư vấn này đã bị đánh dấu Quá hạn do quá lâu không được xác nhận hoàn thành." });
            }

            if (meeting.Status != "Scheduled")
            {
                return Json(new { success = false, message = "Buổi tư vấn này đã được xử lý trước đó, không thể cập nhật lại." });
            }

            var earliestAllowedTime = meeting.ScheduledTime.AddMinutes(-EarlyCompleteWindowMinutes);
            if (DateTime.Now < earliestAllowedTime)
            {
                return Json(new { success = false, message = $"Chưa thể đánh dấu hoàn thành. Bạn có thể xác nhận sớm nhất từ {earliestAllowedTime:HH:mm dd/MM/yyyy} ({EarlyCompleteWindowMinutes} phút trước giờ hẹn)." });
            }

            meeting.Status = "Completed";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Buổi tư vấn đã được đánh dấu hoàn thành thành công." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNoShow(int meetingId)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Json(new { success = false, message = "Bạn chưa đăng nhập." });
            var userId = int.Parse(userIdValue);

            var meeting = await _context.MentorshipMeetings.FindAsync(meetingId);
            if (meeting == null || meeting.MentorId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy buổi tư vấn." });
            }

            if (meeting.Status != "Scheduled")
            {
                return Json(new { success = false, message = "Chỉ có thể đánh dấu Vắng mặt cho các buổi hẹn đang ở trạng thái Đã lên lịch." });
            }

            if (meeting.ScheduledTime > DateTime.Now)
            {
                return Json(new { success = false, message = "Chưa thể đánh dấu Vắng mặt vì buổi tư vấn chưa đến giờ hẹn." });
            }

            meeting.Status = "NoShow";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã đánh dấu buổi tư vấn là Vắng mặt." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyReview(int reviewId, string replyComment)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Json(new { success = false, message = "Bạn cần đăng nhập." });
            var userId = int.Parse(userIdValue);

            var review = await _context.MentorReviews.FindAsync(reviewId);
            if (review == null || review.MentorId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá hợp lệ." });
            }

            review.ReplyComment = replyComment ?? string.Empty;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Phản hồi đánh giá thành công!" });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var meetings = await _context.MentorshipMeetings
                .Include(mm => mm.Mentee)
                .Where(mm => mm.MentorId == userId)
                .OrderByDescending(mm => mm.ScheduledTime)
                .ToListAsync();

            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Tuan");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Lịch hẹn tư vấn");

                worksheet.Cells[1, 1].Value = "ID Cuộc hẹn";
                worksheet.Cells[1, 2].Value = "Chủ đề";
                worksheet.Cells[1, 3].Value = "Mô tả";
                worksheet.Cells[1, 4].Value = "Học viên";
                worksheet.Cells[1, 5].Value = "Email Học viên";
                worksheet.Cells[1, 6].Value = "SĐT Học viên";
                worksheet.Cells[1, 7].Value = "Thời gian hẹn";
                worksheet.Cells[1, 8].Value = "Trạng thái";
                worksheet.Cells[1, 9].Value = "Ngày tạo";

                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                int row = 2;
                foreach (var m in meetings)
                {
                    worksheet.Cells[row, 1].Value = m.Id;
                    worksheet.Cells[row, 2].Value = m.Title;
                    worksheet.Cells[row, 3].Value = m.Description;
                    worksheet.Cells[row, 4].Value = m.Mentee?.FullName;
                    worksheet.Cells[row, 5].Value = m.Mentee?.Email;
                    worksheet.Cells[row, 6].Value = m.Mentee?.PhoneNumber;
                    worksheet.Cells[row, 7].Value = m.ScheduledTime.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cells[row, 8].Value = m.Status;
                    worksheet.Cells[row, 9].Value = m.CreatedAt.ToString("dd/MM/yyyy");
                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"LichHen_TuVan_Mentor_{userId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        private async Task CheckUpcomingMeetingsAsync(int userId)
        {
            var now = DateTime.Now;
            var soon = now.AddMinutes(30);

            var upcomingMeetings = await _context.MentorshipMeetings
                .Include(mm => mm.Mentee)
                .Include(mm => mm.Mentor)
                .Where(mm => mm.Status == "Scheduled" 
                             && mm.ScheduledTime > now 
                             && mm.ScheduledTime <= soon
                             && (mm.MenteeId == userId || mm.MentorId == userId))
                .ToListAsync();

            if (upcomingMeetings.Count == 0) return;

            bool databaseChanged = false;

            foreach (var meeting in upcomingMeetings)
            {
                if (meeting.MenteeId == userId)
                {
                    var msg = $"Lịch hẹn sắp diễn ra: \"{meeting.Title}\" lúc {meeting.ScheduledTime:HH:mm}. Hãy chuẩn bị vào phòng học.";
                    var exists = await _context.Notifications
                        .AnyAsync(n => n.UserId == userId && n.Message.Contains(meeting.Title) && n.Message.Contains("Lịch hẹn sắp diễn ra"));

                    if (!exists)
                    {
                        var notification = new Notification
                        {
                            UserId = userId,
                            Message = msg,
                            IsRead = false,
                            CreatedAt = now
                        };
                        _context.Notifications.Add(notification);
                        databaseChanged = true;

                        try { await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", msg); } catch {}
                    }
                }

                if (meeting.MentorId == userId)
                {
                    var msg = $"Lịch hẹn sắp diễn ra: \"{meeting.Title}\" với học viên lúc {meeting.ScheduledTime:HH:mm}.";
                    var exists = await _context.Notifications
                        .AnyAsync(n => n.UserId == userId && n.Message.Contains(meeting.Title) && n.Message.Contains("Lịch hẹn sắp diễn ra"));

                    if (!exists)
                    {
                        var notification = new Notification
                        {
                            UserId = userId,
                            Message = msg,
                            IsRead = false,
                            CreatedAt = now
                        };
                        _context.Notifications.Add(notification);
                        databaseChanged = true;

                        try { await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", msg); } catch {}
                    }
                }
            }

            if (databaseChanged)
            {
                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await CheckUpcomingMeetingsAsync(user.Id);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new {
                    n.Id,
                    n.Message,
                    n.IsRead,
                    CreatedAt = n.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unread = await _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
    }
}
