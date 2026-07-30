using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using OfficeOpenXml;
using System.IO;

namespace Career_Guidance_Platform.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public DashboardController(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private DateTime? GetCutoffDate(string? timeRange)
        {
            if (string.IsNullOrEmpty(timeRange)) return null;
            return timeRange.ToLower() switch
            {
                "1day" => DateTime.Now.AddDays(-1),
                "1week" => DateTime.Now.AddDays(-7),
                "1month" => DateTime.Now.AddMonths(-30),
                _ => null
            };
        }

        public async Task<IActionResult> Index(string? role, string? timeRange)
        {
            var stats = await GetStatsDataAsync(role, timeRange);
            ViewBag.TotalUsers = stats.TotalUsers;
            ViewBag.TotalMentors = stats.TotalMentors;
            ViewBag.TotalStudents = stats.TotalStudents;
            ViewBag.TotalMeetings = stats.TotalMeetings;

            var meetingsQuery = _context.MentorshipMeetings
                .Include(m => m.Mentor)
                .ThenInclude(m => m.MentorProfile)
                .Include(m => m.Mentee)
                .AsQueryable();

            var cutoff = GetCutoffDate(timeRange);
            if (cutoff.HasValue)
            {
                meetingsQuery = meetingsQuery.Where(m => m.CreatedAt >= cutoff.Value);
            }

            if (!string.IsNullOrEmpty(role))
            {
                var roleLower = role.ToLower();
                if (roleLower == "student" || roleLower == "user")
                {
                    meetingsQuery = meetingsQuery.Where(m => m.Mentee != null && m.Mentee.Role == "Student");
                }
                else if (roleLower == "mentor")
                {
                    meetingsQuery = meetingsQuery.Where(m => m.Mentor != null && m.Mentor.Role == "Mentor");
                }
            }

            var recentMeetings = await meetingsQuery
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.SelectedRole = role;
            ViewBag.SelectedTimeRange = timeRange;

            return View(recentMeetings);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveStats(string? role, string? timeRange)
        {
            var stats = await GetStatsDataAsync(role, timeRange);
            
            var cutoff = GetCutoffDate(timeRange);
            var meetingsQuery = _context.MentorshipMeetings
                .Include(m => m.Mentor)
                .Include(m => m.Mentee)
                .AsQueryable();

            if (cutoff.HasValue)
            {
                meetingsQuery = meetingsQuery.Where(m => m.CreatedAt >= cutoff.Value);
            }

            if (!string.IsNullOrEmpty(role))
            {
                var roleLower = role.ToLower();
                if (roleLower == "student" || roleLower == "user")
                {
                    meetingsQuery = meetingsQuery.Where(m => m.Mentee != null && m.Mentee.Role == "Student");
                }
                else if (roleLower == "mentor")
                {
                    meetingsQuery = meetingsQuery.Where(m => m.Mentor != null && m.Mentor.Role == "Mentor");
                }
            }

            var recentMeetings = await meetingsQuery
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .Select(m => new
                {
                    title = m.Title,
                    description = m.Description,
                    mentorName = m.Mentor != null ? m.Mentor.FullName : "N/A",
                    mentorEmail = m.Mentor != null ? m.Mentor.Email : "",
                    menteeName = m.Mentee != null ? m.Mentee.FullName : "N/A",
                    menteeEmail = m.Mentee != null ? m.Mentee.Email : "",
                    menteePhone = m.Mentee != null ? (m.Mentee.PhoneNumber ?? "Chưa cung cấp") : "Chưa cung cấp",
                    time = m.ScheduledTime.ToString("dd/MM/yyyy HH:mm"),
                    createdAt = m.CreatedAt.ToString("dd/MM/yyyy"),
                    status = m.Status,
                    statusText = m.Status == "Scheduled" ? "Đã lên lịch" : m.Status == "Completed" ? "Hoàn thành" : m.Status == "NoShow" ? "Vắng mặt" : m.Status,
                    statusClass = m.Status == "Scheduled" ? "bg-blue-lt" : m.Status == "Completed" ? "bg-green-lt" : m.Status == "NoShow" ? "bg-red-lt" : "bg-secondary-lt"
                })
                .ToListAsync();

            return Json(new
            {
                totalUsers = stats.TotalUsers,
                totalMentors = stats.TotalMentors,
                totalStudents = stats.TotalStudents,
                totalMeetings = stats.TotalMeetings,
                meetings = recentMeetings
            });
        }

        private async Task<LiveStatsViewModel> GetStatsDataAsync(string? role, string? timeRange)
        {
            var cutoff = GetCutoffDate(timeRange);

            var usersQuery = _context.Users.AsQueryable();
            var mentorsQuery = _context.MentorProfiles.Include(m => m.User).AsQueryable();
            var meetingsQuery = _context.MentorshipMeetings.AsQueryable();

            if (cutoff.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.CreatedAt >= cutoff.Value);
                mentorsQuery = mentorsQuery.Where(m => m.User != null && m.User.CreatedAt >= cutoff.Value);
                meetingsQuery = meetingsQuery.Where(m => m.CreatedAt >= cutoff.Value);
            }

            if (!string.IsNullOrEmpty(role))
            {
                var roleLower = role.ToLower();
                if (roleLower == "student" || roleLower == "user")
                {
                    usersQuery = usersQuery.Where(u => u.Role == "Student");
                    mentorsQuery = mentorsQuery.Where(m => false);
                }
                else if (roleLower == "mentor")
                {
                    usersQuery = usersQuery.Where(u => u.Role == "Mentor");
                }
                else if (roleLower == "admin")
                {
                    usersQuery = usersQuery.Where(u => u.Role == "Admin");
                    mentorsQuery = mentorsQuery.Where(m => false);
                }
            }

            var totalUsers = await usersQuery.CountAsync();
            var totalMentors = await mentorsQuery.CountAsync();
            var totalStudents = await usersQuery.CountAsync(u => u.Role == "Student");
            var totalMeetings = await meetingsQuery.CountAsync();

            return new LiveStatsViewModel
            {
                TotalUsers = totalUsers,
                TotalMentors = totalMentors,
                TotalStudents = totalStudents,
                TotalMeetings = totalMeetings
            };
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var meetings = await _context.MentorshipMeetings
                .Include(mm => mm.Mentor)
                .Include(mm => mm.Mentee)
                .OrderByDescending(mm => mm.ScheduledTime)
                .ToListAsync();

            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Tuan");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tất cả lịch hẹn tư vấn");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Chủ đề";
                worksheet.Cells[1, 3].Value = "Cố vấn";
                worksheet.Cells[1, 4].Value = "Học viên";
                worksheet.Cells[1, 5].Value = "Thời gian hẹn";
                worksheet.Cells[1, 6].Value = "Trạng thái";
                worksheet.Cells[1, 7].Value = "Ngày tạo";

                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                }

                int row = 2;
                foreach (var m in meetings)
                {
                    worksheet.Cells[row, 1].Value = m.Id;
                    worksheet.Cells[row, 2].Value = m.Title;
                    worksheet.Cells[row, 3].Value = m.Mentor?.FullName;
                    worksheet.Cells[row, 4].Value = m.Mentee?.FullName;
                    worksheet.Cells[row, 5].Value = m.ScheduledTime.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cells[row, 6].Value = m.Status;
                    worksheet.Cells[row, 7].Value = m.CreatedAt.ToString("dd/MM/yyyy");
                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"All_LichHen_TuVan_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
    }

    public class LiveStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalMentors { get; set; }
        public int TotalStudents { get; set; }
        public int TotalMeetings { get; set; }
    }
}
