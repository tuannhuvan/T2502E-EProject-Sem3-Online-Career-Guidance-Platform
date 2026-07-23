using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Career_Guidance_Platform.Filters
{
    public class PremiumAccessFilter : IAsyncActionFilter
    {
        private readonly UserManager<User> _userManager;

        public PremiumAccessFilter(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userPrincipal = context.HttpContext.User;
            if (userPrincipal != null && userPrincipal.Identity != null && userPrincipal.Identity.IsAuthenticated)
            {
                // Bỏ qua giới hạn đối với Admin và Mentor
                if (userPrincipal.IsInRole("Admin") || userPrincipal.IsInRole("Mentor"))
                {
                    await next();
                    return;
                }

                var userIdValue = _userManager.GetUserId(userPrincipal);
                if (!string.IsNullOrEmpty(userIdValue))
                {
                    var user = await _userManager.FindByIdAsync(userIdValue);
                    if (user != null)
                    {
                        // Nếu tài khoản không phải Premium và số lần làm test đã từ 3 trở lên
                        if (!user.IsPremium && user.TestAttemptsCount >= 3)
                        {
                            var controller = context.Controller as Controller;
                            if (controller != null)
                            {
                                controller.TempData["PremiumLimitMessage"] = "Bạn đã đạt giới hạn 3 lần thực hiện bài trắc nghiệm đối với tài khoản miễn phí. Vui lòng nâng cấp lên gói thành viên Premium để mở khóa không giới hạn lượt làm test, lộ trình và kho học liệu.";
                            }

                            // Điều hướng người dùng đến trang nâng cấp Premium
                            context.Result = new RedirectToActionResult("UpgradePremium", "Home", null);
                            return;
                        }
                    }
                }
            }

            await next();
        }
    }
}
