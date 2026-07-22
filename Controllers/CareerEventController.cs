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
    public class CareerEventController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public CareerEventController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /CareerEvent
        public async Task<IActionResult> Index()
        {
            var events = await _context.CareerEvents
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            var regCounts = new Dictionary<int, int>();
            foreach (var ev in events)
            {
                regCounts[ev.Id] = await _context.EventRegistrations.CountAsync(er => er.EventId == ev.Id);
            }

            ViewBag.RegistrationCounts = regCounts;
            return View(events);
        }

        // GET: /CareerEvent/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var careerEvent = await _context.CareerEvents.FindAsync(id);
            if (careerEvent == null)
            {
                return NotFound("Không tìm thấy sự kiện này.");
            }

            var registeredCount = await _context.EventRegistrations
                .CountAsync(er => er.EventId == id);

            bool isRegistered = false;
            var userIdValue = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userIdValue))
            {
                var userId = int.Parse(userIdValue);
                isRegistered = await _context.EventRegistrations
                    .AnyAsync(er => er.EventId == id && er.UserId == userId);
            }

            ViewBag.RegisteredCount = registeredCount;
            ViewBag.IsRegistered = isRegistered;

            return View(careerEvent);
        }

        // POST: /CareerEvent/RegisterEvent/{id}
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RegisterEvent(int id)
        {
            var userIdValue = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdValue))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var userId = int.Parse(userIdValue);
            var careerEvent = await _context.CareerEvents.FindAsync(id);
            if (careerEvent == null)
            {
                return Json(new { success = false, message = "Sự kiện không tồn tại." });
            }

            var user = await _userManager.FindByIdAsync(userIdValue);
            bool isPremium = user?.IsPremium == true || await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Mentor");

            // Check VIP exclusive events
            bool isVipEvent = careerEvent.Title.Contains("VIP", StringComparison.OrdinalIgnoreCase) || 
                              careerEvent.Title.Contains("Premium", StringComparison.OrdinalIgnoreCase) ||
                              careerEvent.Location.Contains("VIP", StringComparison.OrdinalIgnoreCase) ||
                              careerEvent.Location.Contains("Premium", StringComparison.OrdinalIgnoreCase);

            if (isVipEvent && !isPremium)
            {
                return Json(new { success = false, message = "Sự kiện này chỉ dành riêng cho tài khoản Premium VIP. Vui lòng nâng cấp tài khoản để tham gia!" });
            }

            // Check if already registered
            var alreadyRegistered = await _context.EventRegistrations
                .AnyAsync(er => er.EventId == id && er.UserId == userId);

            if (alreadyRegistered)
            {
                return Json(new { success = false, message = "Bạn đã đăng ký tham gia sự kiện này rồi." });
            }

            // Check event capacity
            var registeredCount = await _context.EventRegistrations
                .CountAsync(er => er.EventId == id);

            if (registeredCount >= careerEvent.MaxParticipants)
            {
                return Json(new { success = false, message = "Rất tiếc! Sự kiện đã đạt số lượng người tham gia tối đa." });
            }

            // Check deadline
            if (careerEvent.EventDate < DateTime.Now)
            {
                return Json(new { success = false, message = "Hạn đăng ký đã kết thúc vì sự kiện đã diễn ra." });
            }

            // Add registration record
            var registration = new EventRegistration
            {
                EventId = id,
                UserId = userId,
                RegisteredAt = DateTime.Now,
                IsVip = isPremium
            };
            _context.EventRegistrations.Add(registration);

            // Create notification
            var notification = new Notification
            {
                UserId = userId,
                Message = $"Đăng ký thành công sự kiện '{careerEvent.Title}'. Thời gian: {careerEvent.EventDate:dd/MM/yyyy HH:mm}.",
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đăng ký tham gia sự kiện thành công!" });
        }
    }
}
