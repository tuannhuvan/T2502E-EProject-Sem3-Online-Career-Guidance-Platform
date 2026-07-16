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

namespace Career_Guidance_Platform.Controllers
{
    [Authorize]
    public class MentorshipController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public MentorshipController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. MENTEE VIEW: Danh sách Mentor hỗ trợ tìm kiếm & xếp hạng thông minh
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search, string? skill, string? careerPath)
        {
            // Lấy tất cả các Mentor trong database
            var query = _context.MentorProfiles
                .Include(m => m.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.User.FullName.Contains(search) || 
                                         m.JobTitle.Contains(search) || 
                                         m.Biography.Contains(search));
            }

            if (!string.IsNullOrEmpty(skill))
            {
                query = query.Where(m => m.Expertise.Contains(skill) || m.Specialization.Contains(skill));
            }

            if (!string.IsNullOrEmpty(careerPath))
            {
                query = query.Where(m => m.JobTitle.Contains(careerPath) || m.Specialization.Contains(careerPath));
            }

            // 1. Lấy danh sách kỹ năng mong muốn của Mentee (User hiện tại)
            var targetSkills = new List<Skill>();
            var userIdValue = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userIdValue))
            {
                var userId = int.Parse(userIdValue);
                
                // 1.1 Lấy các skill từ danh sách kỹ năng của người học
                var userSkills = await _context.UserSkills
                    .Include(us => us.Skill)
                    .Where(us => us.UserId == userId && us.Skill != null)
                    .Select(us => us.Skill!)
                    .ToListAsync();
                targetSkills.AddRange(userSkills);

                // 1.2 Lấy các skill từ lộ trình học tập của các Goal hiện tại
                var goalCareerPathIds = await _context.Goals
                    .Where(g => g.StudentId == userId && g.CareerPathId != null && g.Status == 1)
                    .Select(g => g.CareerPathId!.Value)
                    .ToListAsync();

                if (goalCareerPathIds.Any())
                {
                    var careerPathSkills = await _context.Set<CareerPathSkill>()
                        .Include(cps => cps.Skill)
                        .Where(cps => goalCareerPathIds.Contains(cps.CareerPathId) && cps.Skill != null)
                        .Select(cps => cps.Skill!)
                        .ToListAsync();
                    targetSkills.AddRange(careerPathSkills);
                }

                // Loại bỏ trùng lặp kỹ năng
                targetSkills = targetSkills.GroupBy(s => s.Id).Select(g => g.First()).ToList();
            }

            var mentorsList = await query.ToListAsync();

            // Tính điểm uy tín MentorScore in-memory để tránh lỗi biên dịch hàm Log của SQL
            var rankedMentors = mentorsList.Select(m =>
            {
                var totalCompletedSessions = _context.MentorshipMeetings
                    .Count(mm => mm.MentorId == m.UserId && mm.Status == "Completed");

                // Tính toán trung bình rating từ bảng mentor_reviews
                var ratings = _context.MentorReviews
                    .Where(mr => mr.MentorId == m.UserId)
                    .Select(mr => mr.Rating)
                    .ToList();

                double averageRating = ratings.Any() ? ratings.Average() : 5.0; // Mặc định là 5 sao nếu chưa có đánh giá

                // Tính các kỹ năng khớp giữa Mentee và Mentor
                var matchedSkills = new List<string>();
                if (targetSkills.Any())
                {
                    foreach (var ts in targetSkills)
                    {
                        bool matchesExp = !string.IsNullOrEmpty(m.Expertise) && m.Expertise.Contains(ts.Name, StringComparison.OrdinalIgnoreCase);
                        bool matchesSpec = !string.IsNullOrEmpty(m.Specialization) && m.Specialization.Contains(ts.Name, StringComparison.OrdinalIgnoreCase);
                        if (matchesExp || matchesSpec)
                        {
                            matchedSkills.Add(ts.Name);
                        }
                    }
                }

                double matchBoost = matchedSkills.Count * 1.5; // Cộng 1.5 điểm mỗi kỹ năng khớp để ưu tiên đề xuất lên đầu
                double score = (averageRating * 0.7) + (Math.Log(totalCompletedSessions + 1) * 0.3) + matchBoost;

                return new RankedMentorViewModel
                {
                    Profile = m,
                    AverageRating = averageRating,
                    TotalReviews = ratings.Count,
                    TotalSessionsCompleted = totalCompletedSessions,
                    RankScore = score,
                    MatchedSkills = matchedSkills
                };
            })
            .OrderByDescending(rm => rm.RankScore)
            .ToList();

            ViewBag.Search = search;
            ViewBag.Skill = skill;
            ViewBag.CareerPath = careerPath;

            // Lấy danh mục tất cả Kỹ năng và Lộ trình nghề nghiệp để hiển thị trên bộ lọc
            ViewBag.Skills = await _context.Skills.Where(s => s.Status == 1).Select(s => s.Name).Distinct().ToListAsync();
            ViewBag.CareerPathsList = await _context.CareerPaths.Where(cp => cp.Status == 1).Select(cp => cp.Title).ToListAsync();

            return View(rankedMentors);
        }

        // 2. MENTEE VIEW: Chi tiết thông tin Cố vấn & Các đánh giá
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var mentor = await _context.MentorProfiles
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (mentor == null)
            {
                return NotFound("Không tìm thấy thông tin cố vấn.");
            }

            // Lấy danh sách đánh giá của cố vấn này
            var reviews = await _context.MentorReviews
                .Include(r => r.Mentee)
                .Where(r => r.MentorId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Tính số lượng các buổi tư vấn đã hoàn thành
            var completedSessionsCount = await _context.MentorshipMeetings
                .CountAsync(mm => mm.MentorId == id && mm.Status == "Completed");

            double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 5.0;

            ViewBag.Reviews = reviews;
            ViewBag.CompletedSessions = completedSessionsCount;
            ViewBag.AverageRating = averageRating;

            // Kiểm tra trạng thái yêu cầu hiện tại của học viên
            var userIdValue = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userIdValue))
            {
                var userId = int.Parse(userIdValue);
                ViewBag.CurrentRequest = await _context.MentorshipRequests
                    .FirstOrDefaultAsync(mr => mr.MenteeId == userId && mr.MentorId == id);

                var reviewedMeetingIds = await _context.MentorReviews
                    .Where(mr => mr.MenteeId == userId)
                    .Select(mr => mr.MeetingId)
                    .ToListAsync();

                ViewBag.MenteeMeetings = await _context.MentorshipMeetings
                    .Where(mm => mm.MenteeId == userId && mm.MentorId == id && !reviewedMeetingIds.Contains(mm.Id))
                    .OrderByDescending(mm => mm.ScheduledTime)
                    .ToListAsync();
            }

            // Lấy danh sách lịch bận (đã lên lịch) của Mentor này
            var mentorBusyTimes = await _context.MentorshipMeetings
                .Where(mm => mm.MentorId == id && mm.Status == "Scheduled")
                .Select(mm => mm.ScheduledTime)
                .ToListAsync();
            ViewBag.MentorBusyTimes = mentorBusyTimes.Select(t => t.ToString("yyyy-MM-dd HH:mm")).ToList();

            return View(mentor);
        }

        // 3. MENTEE ACTION: Gửi yêu cầu kết nối Mentorship
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestMentorship(int mentorId, string message)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            // Kiểm tra xem đã gửi yêu cầu trước đó chưa
            var existing = await _context.MentorshipRequests
                .FirstOrDefaultAsync(mr => mr.MenteeId == userId && mr.MentorId == mentorId);

            if (existing != null)
            {
                TempData["MessageWarning"] = "Bạn đã gửi yêu cầu kết nối đến Cố vấn này trước đó rồi!";
                return RedirectToAction(nameof(Details), new { id = mentorId });
            }

            var request = new MentorshipRequest
            {
                MenteeId = userId,
                MentorId = mentorId,
                Message = message,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.MentorshipRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["MessageSuccess"] = "Yêu cầu kết nối của bạn đã được gửi thành công! Hãy đợi phản hồi từ cố vấn.";
            return RedirectToAction(nameof(Details), new { id = mentorId });
        }

        // 4. MENTEE ACTION: Đặt lịch tư vấn 1-1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookMentor(int mentorId, string meetingDate, string meetingTime, string notes)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var mentorProfile = await _context.MentorProfiles.Include(m => m.User).FirstOrDefaultAsync(m => m.UserId == mentorId);
            if (mentorProfile == null)
            {
                return NotFound("Không tìm thấy thông tin Mentor.");
            }

            // Kiểm tra xem Mentee đã được kết nối với Mentor hay chưa (yêu cầu phải kết nối trước khi đặt lịch)
            var connection = await _context.MentorshipRequests
                .FirstOrDefaultAsync(mr => mr.MenteeId == userId && mr.MentorId == mentorId && mr.Status == "Approved");

            if (connection == null)
            {
                TempData["MessageWarning"] = "Bạn cần gửi yêu cầu kết nối và được Cố vấn phê duyệt trước khi đặt lịch hẹn tư vấn!";
                return RedirectToAction(nameof(Details), new { id = mentorId });
            }

            var parsedTime = DateTime.Parse($"{meetingDate} {meetingTime}");

            var meeting = new MentorshipMeeting
            {
                MenteeId = userId,
                MentorId = mentorId,
                Title = $"Tư vấn định hướng nghề nghiệp cùng Mentor {mentorProfile.User?.FullName}",
                Description = notes,
                ScheduledTime = parsedTime,
                MeetingUrl = $"https://meet.jit.si/CareerPathMentorshipMeeting_{Guid.NewGuid().ToString().Substring(0, 8)}", // Link họp Jitsi thật
                Status = "Scheduled",
                CreatedAt = DateTime.Now
            };

            _context.MentorshipMeetings.Add(meeting);
            await _context.SaveChangesAsync();

            TempData["BookingSuccess"] = $"Đặt lịch hẹn tư vấn thành công lúc {parsedTime:dd/MM/yyyy HH:mm}! Link phòng họp: {meeting.MeetingUrl}";
            return RedirectToAction(nameof(Details), new { id = mentorId });
        }

        // 5. MENTOR ACTION: Quản lý Yêu cầu & Lịch hẹn (Dashboard)
        public async Task<IActionResult> Dashboard()
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var mentorProfile = await _context.MentorProfiles.FirstOrDefaultAsync(m => m.UserId == userId);
            if (mentorProfile == null)
            {
                return RedirectToAction("Index", "Home");
            }

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
                .OrderByDescending(mm => mm.ScheduledTime)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.Meetings = meetings;

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

            request.Status = status; // Approved, Rejected, Cancelled
            await _context.SaveChangesAsync();

            TempData["DashboardSuccess"] = $"Đã cập nhật trạng thái yêu cầu kết nối thành: {status}";
            return RedirectToAction(nameof(Dashboard));
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

            meeting.Status = "Completed";
            await _context.SaveChangesAsync();

            // Kích hoạt thông báo đánh giá cho mentee bằng TempData
            TempData["DashboardSuccess"] = "Buổi tư vấn đã được đánh dấu hoàn thành. Hệ thống sẽ mở form đánh giá cho người học.";
            return RedirectToAction(nameof(Dashboard));
        }

        // 6. MENTEE VIEW & ACTION: Viết Đánh giá
        public async Task<IActionResult> WriteReview(int meetingId)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var meeting = await _context.MentorshipMeetings
                .Include(mm => mm.Mentor)
                .ThenInclude(m => m.MentorProfile)
                .FirstOrDefaultAsync(mm => mm.Id == meetingId);

            if (meeting == null || meeting.MenteeId != userId)
            {
                return NotFound("Không tìm thấy buổi gặp hoặc bạn không có quyền đánh giá.");
            }

            if (meeting.Status != "Completed")
            {
                return BadRequest("Buổi gặp chưa hoàn thành, không thể đánh giá.");
            }

            // Kiểm tra xem đã có review nào chưa
            var existingReview = await _context.MentorReviews
                .AnyAsync(r => r.MeetingId == meetingId);

            if (existingReview)
            {
                TempData["MessageWarning"] = "Bạn đã thực hiện đánh giá cho buổi tư vấn này rồi!";
                return RedirectToAction(nameof(Details), new { id = meeting.MentorId });
            }

            return View(meeting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int meetingId, int rating, string comment)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var meeting = await _context.MentorshipMeetings.FindAsync(meetingId);
            if (meeting == null || meeting.MenteeId != userId)
            {
                return NotFound("Không tìm thấy buổi gặp hợp lệ.");
            }

            if (meeting.Status != "Completed")
            {
                return BadRequest("Chỉ được đánh giá buổi gặp đã hoàn thành.");
            }

            // Tạo mới review
            var review = new MentorReview
            {
                MeetingId = meetingId,
                MentorId = meeting.MentorId,
                MenteeId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.MentorReviews.Add(review);
            await _context.SaveChangesAsync();

            // Background update: Tính toán và cập nhật lại Rating trung bình vào cột Rating của MentorProfile
            var allReviewsForMentor = await _context.MentorReviews
                .Where(r => r.MentorId == meeting.MentorId)
                .Select(r => r.Rating)
                .ToListAsync();

            decimal averageRating = (decimal)allReviewsForMentor.Average();

            var mentorProfile = await _context.MentorProfiles.FindAsync(meeting.MentorId);
            if (mentorProfile != null)
            {
                mentorProfile.Rating = averageRating;
                await _context.SaveChangesAsync();
            }

            TempData["ReviewSuccess"] = "Cảm ơn bạn đã phản hồi đánh giá chất lượng Cố vấn!";
            return RedirectToAction(nameof(Details), new { id = meeting.MentorId });
        }

        // 7. GROUP MENTORING SESSIONS: Xem danh sách và đăng ký
        public async Task<IActionResult> GroupSessions()
        {
            var userIdValue = _userManager.GetUserId(User);
            var userId = !string.IsNullOrEmpty(userIdValue) ? int.Parse(userIdValue) : 0;

            var sessions = await _context.GroupMentoringSessions
                .Include(s => s.Mentor)
                .Include(s => s.Registrations)
                .Where(s => s.Status == "Scheduled" && s.ScheduledTime > DateTime.Now)
                .OrderBy(s => s.ScheduledTime)
                .ToListAsync();

            ViewBag.RegisteredSessionIds = await _context.GroupMentoringRegistrations
                .Where(r => r.StudentId == userId)
                .Select(r => r.SessionId)
                .ToListAsync();

            return View(sessions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterGroupSession(int sessionId)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Challenge();
            var userId = int.Parse(userIdValue);

            var session = await _context.GroupMentoringSessions
                .Include(s => s.Registrations)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                return NotFound("Không tìm thấy buổi hội thảo.");
            }

            // Kiểm tra số lượng người tham gia tối đa
            if (session.Registrations.Count >= session.MaxParticipants)
            {
                TempData["GroupWarning"] = "Buổi tư vấn nhóm này đã đủ số lượng người đăng ký!";
                return RedirectToAction(nameof(GroupSessions));
            }

            // Kiểm tra đã đăng ký chưa
            var existingReg = await _context.GroupMentoringRegistrations
                .FirstOrDefaultAsync(r => r.SessionId == sessionId && r.StudentId == userId);

            if (existingReg != null)
            {
                TempData["GroupWarning"] = "Bạn đã đăng ký tham gia hội thảo này từ trước rồi.";
                return RedirectToAction(nameof(GroupSessions));
            }

            var reg = new GroupMentoringRegistration
            {
                SessionId = sessionId,
                StudentId = userId,
                RegisteredAt = DateTime.Now
            };

            _context.GroupMentoringRegistrations.Add(reg);
            await _context.SaveChangesAsync();

            TempData["GroupSuccess"] = $"Đăng ký tham gia buổi tư vấn nhóm '{session.Title}' thành công!";
            return RedirectToAction(nameof(GroupSessions));
        }
    }

    // ViewModel phục vụ hiển thị Mentor cùng điểm Rank
    public class RankedMentorViewModel
    {
        public MentorProfile Profile { get; set; } = null!;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalSessionsCompleted { get; set; }
        public double RankScore { get; set; }
        public List<string> MatchedSkills { get; set; } = new List<string>();
        public bool IsRecommended => MatchedSkills.Any();
    }
}
