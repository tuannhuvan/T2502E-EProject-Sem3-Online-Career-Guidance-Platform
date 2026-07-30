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
using Microsoft.AspNetCore.SignalR;
using Career_Guidance_Platform.Hubs;

namespace Career_Guidance_Platform.Controllers
{
    [Authorize]
    public class MentorshipController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public MentorshipController(AppDbContext context, UserManager<User> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        private async Task<bool> IsPremiumUserAsync()
        {
            if (User.Identity?.IsAuthenticated != true) return false;
            if (User.IsInRole("Admin") || User.IsInRole("Mentor")) return true;
            
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return false;
            
            var user = await _userManager.FindByIdAsync(userIdValue);
            return user?.IsPremium == true;
        }

        // Cấu hình vòng đời buổi tư vấn
        private const int EarlyCompleteWindowMinutes = 30; // Cho phép Mentor bấm "Hoàn thành" sớm tối đa 30 phút trước giờ hẹn
        private const int ExpireGraceHours = 3;             // Quá 3 tiếng kể từ giờ hẹn mà chưa ai xác nhận -> tự động "Expired"

        // Tự động chuyển các lịch hẹn quá giờ quá lâu mà không ai bấm "Hoàn thành" sang trạng thái "Expired".
        // Được gọi ở đầu Dashboard()/Details() để dữ liệu luôn đúng thực tế mỗi lần người dùng ghé trang,
        // không cần dựng thêm background job/scheduler riêng.
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
                meeting.Status = "Expired"; // Quá hạn: không bên nào xác nhận hoàn thành trong thời gian ân hạn
            }

            await _context.SaveChangesAsync();
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

                // Quét và tự động đánh dấu "Expired" các lịch hẹn quá hạn trước khi hiển thị cho học viên
                await AutoExpireOverdueMeetingsAsync(menteeId: userId);

                ViewBag.CurrentRequest = await _context.MentorshipRequests
                    .FirstOrDefaultAsync(mr => mr.MenteeId == userId && mr.MentorId == id);

                var reviewedMeetingIds = await _context.MentorReviews
                    .Where(mr => mr.MenteeId == userId)
                    .Select(mr => mr.MeetingId)
                    .ToListAsync();
                ViewBag.ReviewedMeetingIds = reviewedMeetingIds;

                var menteeReviews = await _context.MentorReviews
                    .Where(mr => mr.MenteeId == userId && mr.MentorId == id)
                    .ToDictionaryAsync(mr => mr.MeetingId);
                ViewBag.MenteeReviews = menteeReviews;

                ViewBag.MenteeMeetings = await _context.MentorshipMeetings
                    .Where(mm => mm.MenteeId == userId && mm.MentorId == id)
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

            // Chặn tài khoản Mentor gửi yêu cầu kết nối như học viên, và chặn tự kết nối với chính mình
            var isMentorAccount = User.IsInRole("Mentor") || await _context.MentorProfiles.AnyAsync(mp => mp.UserId == userId);
            if (isMentorAccount || userId == mentorId)
            {
                TempData["MessageWarning"] = "Tài khoản Cố vấn (Mentor) không thể gửi yêu cầu kết nối như học viên, hoặc bạn không thể tự kết nối với chính mình.";
                return RedirectToAction("Index", "Home");
            }

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

            // Send notification to Mentor
            var menteeUser = await _userManager.GetUserAsync(User);
            var menteeName = menteeUser?.FullName ?? "Học viên";
            var msg = $"Học viên {menteeName} đã gửi yêu cầu kết nối mới.";
            
            var notification = new Notification
            {
                UserId = mentorId,
                Message = msg,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(mentorId.ToString()).SendAsync("ReceiveNotification", msg);

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

            // Chặn tài khoản Mentor tự đặt lịch như học viên, và chặn tự đặt lịch với chính mình
            var isMentorAccount = User.IsInRole("Mentor") || await _context.MentorProfiles.AnyAsync(mp => mp.UserId == userId);
            if (isMentorAccount || userId == mentorId)
            {
                TempData["MessageWarning"] = "Tài khoản Cố vấn (Mentor) không thể tự đặt lịch tư vấn như học viên, hoặc bạn không thể tự đặt lịch với chính mình.";
                return RedirectToAction("Index", "Home");
            }

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

            // Free user limit: check scheduled/completed meetings booked in the current month
            var isPremium = await IsPremiumUserAsync();
            if (!isPremium)
            {
                var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);
                var meetingsCount = await _context.MentorshipMeetings
                    .CountAsync(mm => mm.MenteeId == userId && mm.ScheduledTime >= startOfMonth && mm.ScheduledTime < endOfMonth);

                if (meetingsCount >= 2)
                {
                    TempData["MessageWarning"] = "Tài khoản thường (Free) chỉ được đặt lịch tối đa 2 buổi/tháng. Vui lòng nâng cấp Premium để không giới hạn đặt lịch cố vấn!";
                    return RedirectToAction(nameof(Details), new { id = mentorId });
                }
            }

            // Thay thế DateTime.Parse bằng TryParse để tránh lỗi 500 khi input sai định dạng
            if (!DateTime.TryParse($"{meetingDate} {meetingTime}", out var parsedTime))
            {
                TempData["MessageWarning"] = "Ngày giờ đặt lịch không hợp lệ. Vui lòng kiểm tra lại định dạng.";
                return RedirectToAction(nameof(Details), new { id = mentorId });
            }

            // Chặn đặt lịch trong quá khứ
            if (parsedTime <= DateTime.Now)
            {
                TempData["MessageWarning"] = "Không thể đặt lịch hẹn trong quá khứ. Vui lòng chọn thời gian khác.";
                return RedirectToAction(nameof(Details), new { id = mentorId });
            }

            // Kiểm tra trùng lịch (double-booking) của Mentor trong khoảng ±60 phút
            var windowStart = parsedTime.AddMinutes(-60);
            var windowEnd = parsedTime.AddMinutes(60);

            var isMentorBusy = await _context.MentorshipMeetings
                .AnyAsync(mm => mm.MentorId == mentorId
                                && mm.Status == "Scheduled"
                                && mm.ScheduledTime > windowStart
                                && mm.ScheduledTime < windowEnd);

            if (isMentorBusy)
            {
                TempData["MessageWarning"] = "Mentor đã có lịch hẹn khác gần khung giờ này (±60 phút). Vui lòng chọn thời gian khác.";
                return RedirectToAction(nameof(Details), new { id = mentorId });
            }

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

            // Send notification to Mentor
            var menteeUser = await _userManager.GetUserAsync(User);
            var menteeName = menteeUser?.FullName ?? "Học viên";
            var msg = $"Học viên {menteeName} đã đặt lịch hẹn tư vấn mới lúc {parsedTime:dd/MM/yyyy HH:mm}.";

            var notification = new Notification
            {
                UserId = mentorId,
                Message = msg,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(mentorId.ToString()).SendAsync("ReceiveNotification", msg);

            TempData["BookingSuccess"] = $"Đặt lịch hẹn tư vấn thành công lúc {parsedTime:dd/MM/yyyy HH:mm}! Link phòng họp: {meeting.MeetingUrl}";
            return RedirectToAction(nameof(Details), new { id = mentorId });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReview(int meetingId, int rating, string comment)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Json(new { success = false, message = "Bạn cần đăng nhập." });
            var userId = int.Parse(userIdValue);

            var meeting = await _context.MentorshipMeetings.FindAsync(meetingId);
            if (meeting == null || meeting.MenteeId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy buổi gặp hợp lệ." });
            }

            if (meeting.Status != "Completed")
            {
                return Json(new { success = false, message = "Chỉ được đánh giá buổi gặp đã hoàn thành." });
            }

            var existingReview = await _context.MentorReviews.AnyAsync(r => r.MeetingId == meetingId);
            if (existingReview)
            {
                return Json(new { success = false, message = "Bạn đã thực hiện đánh giá cho buổi tư vấn này rồi!" });
            }

            var review = new MentorReview
            {
                MeetingId = meetingId,
                MentorId = meeting.MentorId,
                MenteeId = userId,
                Rating = rating,
                Comment = comment ?? string.Empty,
                CreatedAt = DateTime.Now
            };

            _context.MentorReviews.Add(review);
            await _context.SaveChangesAsync();

            var allReviewsForMentor = await _context.MentorReviews
                .Where(r => r.MentorId == meeting.MentorId)
                .Select(r => r.Rating)
                .ToListAsync();

            decimal averageRating = allReviewsForMentor.Any() ? (decimal)allReviewsForMentor.Average() : 5.00m;

            var mentorProfile = await _context.MentorProfiles.FindAsync(meeting.MentorId);
            if (mentorProfile != null)
            {
                mentorProfile.Rating = averageRating;
                await _context.SaveChangesAsync();
            }

            var menteeUser = await _userManager.GetUserAsync(User);
            var menteeName = menteeUser?.FullName ?? "Học viên";
            await _hubContext.Clients.User(meeting.MentorId.ToString()).SendAsync("ReceiveNewReview", new {
                id = review.Id,
                menteeName = menteeName,
                rating = rating,
                comment = comment ?? string.Empty,
                createdAt = review.CreatedAt.ToString("dd/MM/yyyy"),
                isUpdate = false
            });

            return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReview(int meetingId, int rating, string comment)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue)) return Json(new { success = false, message = "Bạn cần đăng nhập." });
            var userId = int.Parse(userIdValue);

            var meeting = await _context.MentorshipMeetings.FindAsync(meetingId);
            if (meeting == null || meeting.MenteeId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy buổi gặp hợp lệ." });
            }

            var review = await _context.MentorReviews.FirstOrDefaultAsync(r => r.MeetingId == meetingId && r.MenteeId == userId);
            if (review == null)
            {
                return Json(new { success = false, message = "Bạn chưa đánh giá buổi tư vấn này." });
            }

            review.Rating = rating;
            review.Comment = comment ?? string.Empty;
            review.CreatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var allReviewsForMentor = await _context.MentorReviews
                .Where(r => r.MentorId == meeting.MentorId)
                .Select(r => r.Rating)
                .ToListAsync();

            decimal averageRating = allReviewsForMentor.Any() ? (decimal)allReviewsForMentor.Average() : 5.00m;

            var mentorProfile = await _context.MentorProfiles.FindAsync(meeting.MentorId);
            if (mentorProfile != null)
            {
                mentorProfile.Rating = averageRating;
                await _context.SaveChangesAsync();
            }

            var menteeUser = await _userManager.GetUserAsync(User);
            var menteeName = menteeUser?.FullName ?? "Học viên";

            await _hubContext.Clients.User(meeting.MentorId.ToString()).SendAsync("ReceiveNewReview", new {
                id = review.Id,
                menteeName = menteeName,
                rating = rating,
                comment = comment ?? string.Empty,
                createdAt = review.CreatedAt.ToString("dd/MM/yyyy"),
                isUpdate = true
            });

            return Json(new { success = true, message = "Đánh giá của bạn đã được cập nhật thành công!" });
        }

        // 7. GROUP MENTORING SESSIONS: Xem danh sách và đăng ký
        public async Task<IActionResult> GroupSessions()
        {
            if (!await IsPremiumUserAsync())
            {
                if (User.Identity?.IsAuthenticated != true)
                {
                    return Challenge();
                }
                TempData["PremiumLimitMessage"] = "Tính năng kết nối Cố vấn (Mentorship) yêu cầu tài khoản Premium VIP. Vui lòng nâng cấp để tiếp tục.";
                return RedirectToAction("UpgradePremium", "Home");
            }

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

        // GET: /Mentorship/Apply (or /Mentor/Apply)
        [HttpGet("/Mentor/Apply")]
        [HttpGet("/Mentorship/Apply")]
        public async Task<IActionResult> Apply()
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue))
                return Challenge();

            var userId = int.Parse(userIdValue);
            var existingProfile = await _context.MentorProfiles.FindAsync(userId);
            if (existingProfile != null)
            {
                if (existingProfile.IsVerified)
                {
                    TempData["Info"] = "Bạn đã là Cố vấn chính thức trên hệ thống.";
                    return RedirectToAction("Index");
                }
                TempData["Info"] = "Hồ sơ đăng ký cố vấn của bạn đang chờ phê duyệt từ quản trị viên.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userIdValue);
            ViewBag.UserEmail = user?.Email;
            ViewBag.UserPhone = user?.PhoneNumber;

            return View("~/Views/Mentorship/Apply.cshtml");
        }

        // POST: /Mentorship/Apply (or /Mentor/Apply)
        [HttpPost("/Mentor/Apply")]
        [HttpPost("/Mentorship/Apply")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(string jobTitle, string company, string specialization, string biography, string? linkedInUrl, string experienceDescription, string expertise, string phoneNumber, string email)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue))
                return Challenge();

            var userId = int.Parse(userIdValue);
            var existingProfile = await _context.MentorProfiles.FindAsync(userId);
            if (existingProfile != null)
            {
                TempData["Info"] = "Hồ sơ của bạn đã được gửi trước đó.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(jobTitle) || string.IsNullOrWhiteSpace(company) || string.IsNullOrWhiteSpace(specialization) || string.IsNullOrWhiteSpace(biography) || string.IsNullOrWhiteSpace(experienceDescription) || string.IsNullOrWhiteSpace(expertise) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ các thông tin bắt buộc bao gồm Số điện thoại và Email.");
                return View("~/Views/Mentorship/Apply.cshtml");
            }

            // Update user properties
            var user = await _userManager.FindByIdAsync(userIdValue);
            if (user != null)
            {
                user.PhoneNumber = phoneNumber;
                
                if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    var existingUserByEmail = await _userManager.FindByEmailAsync(email);
                    if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
                    {
                        ModelState.AddModelError("", "Email này đã được sử dụng bởi một tài khoản khác.");
                        ViewBag.UserEmail = email;
                        ViewBag.UserPhone = phoneNumber;
                        return View("~/Views/Mentorship/Apply.cshtml");
                    }
                    user.Email = email;
                    user.NormalizedEmail = email.ToUpper();
                    user.UserName = email;
                    user.NormalizedUserName = email.ToUpper();
                }
                
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    ViewBag.UserEmail = email;
                    ViewBag.UserPhone = phoneNumber;
                    return View("~/Views/Mentorship/Apply.cshtml");
                }
            }

            var profile = new MentorProfile
            {
                UserId = userId,
                JobTitle = jobTitle,
                Company = company,
                Specialization = specialization,
                Biography = biography,
                LinkedInUrl = linkedInUrl,
                ExperienceDescription = experienceDescription,
                Expertise = expertise,
                IsActive = true,
                IsVerified = false,
                HourlyRate = 0,
                Rating = 5.00m
            };

            _context.MentorProfiles.Add(profile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký cố vấn thành công! Hồ sơ của bạn đã được gửi và đang chờ ban quản trị kiểm duyệt.";
            return RedirectToAction("Index");
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
