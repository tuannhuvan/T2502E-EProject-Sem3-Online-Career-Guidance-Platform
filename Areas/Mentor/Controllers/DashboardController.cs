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

namespace Career_Guidance_Platform.Areas.Mentor.Controllers
{
    [Area("Mentor")]
    [Authorize(Roles = "Mentor")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var request = await _context.MentorshipRequests.FindAsync(requestId);
            if (request == null || request.MentorId != userId)
            {
                return NotFound("Không tìm thấy yêu cầu.");
            }

            var allowedStatuses = new[] { "Approved", "Rejected", "Cancelled" };
            if (!allowedStatuses.Contains(status))
            {
                TempData["MessageWarning"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != "Pending")
            {
                TempData["MessageWarning"] = "Yêu cầu này đã được xử lý trước đó, không thể thay đổi lại.";
                return RedirectToAction(nameof(Index));
            }

            request.Status = status;
            await _context.SaveChangesAsync();

            TempData["DashboardSuccess"] = $"Đã cập nhật trạng thái yêu cầu kết nối thành: {status}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteMeeting(int meetingId)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var meeting = await _context.MentorshipMeetings.FindAsync(meetingId);
            if (meeting == null || meeting.MentorId != userId)
            {
                return NotFound("Không tìm thấy buổi tư vấn.");
            }

            if (meeting.Status == "Expired" || meeting.Status == "NoShow")
            {
                TempData["MessageWarning"] = "Buổi tư vấn này đã bị đánh dấu Quá hạn do quá lâu không được xác nhận hoàn thành. Vui lòng trao đổi lại với học viên để đặt lịch mới.";
                return RedirectToAction(nameof(Index));
            }

            if (meeting.Status != "Scheduled")
            {
                TempData["MessageWarning"] = "Buổi tư vấn này đã được xử lý trước đó, không thể cập nhật lại.";
                return RedirectToAction(nameof(Index));
            }

            var earliestAllowedTime = meeting.ScheduledTime.AddMinutes(-EarlyCompleteWindowMinutes);
            if (DateTime.Now < earliestAllowedTime)
            {
                TempData["MessageWarning"] = $"Chưa thể đánh dấu hoàn thành. Bạn có thể xác nhận sớm nhất từ {earliestAllowedTime:HH:mm dd/MM/yyyy} ({EarlyCompleteWindowMinutes} phút trước giờ hẹn).";
                return RedirectToAction(nameof(Index));
            }

            meeting.Status = "Completed";
            await _context.SaveChangesAsync();

            TempData["DashboardSuccess"] = "Buổi tư vấn đã được đánh dấu hoàn thành. Hệ thống sẽ mở form đánh giá cho người học.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNoShow(int meetingId)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var meeting = await _context.MentorshipMeetings.FindAsync(meetingId);
            if (meeting == null || meeting.MentorId != userId)
            {
                return NotFound("Không tìm thấy buổi tư vấn.");
            }

            if (meeting.Status != "Scheduled")
            {
                TempData["MessageWarning"] = "Chỉ có thể đánh dấu Vắng mặt cho các buổi hẹn đang ở trạng thái Đã lên lịch.";
                return RedirectToAction(nameof(Index));
            }

            if (meeting.ScheduledTime > DateTime.Now)
            {
                TempData["MessageWarning"] = "Chưa thể đánh dấu Vắng mặt vì buổi tư vấn chưa đến giờ hẹn.";
                return RedirectToAction(nameof(Index));
            }

            meeting.Status = "NoShow";
            await _context.SaveChangesAsync();

            TempData["DashboardSuccess"] = "Đã đánh dấu buổi tư vấn là Vắng mặt (học viên không tham gia).";
            return RedirectToAction(nameof(Index));
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
    }
}
