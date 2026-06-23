using Microsoft.AspNetCore.Mvc;

namespace Career_Guidance_Platform.Controllers;

public class PaymentController : Controller
{
    private const string OrderNumber = "ORD-2026-0001";
    private const decimal Amount = 499000;

    public IActionResult Cart()
    {
        ViewBag.OrderNumber = OrderNumber;
        ViewBag.Amount = Amount;
        ViewBag.Status = "Pending";
        return View();
    }

    public IActionResult Checkout()
    {
        ViewBag.OrderNumber = OrderNumber;
        ViewBag.Amount = Amount;
        ViewBag.Status = "Pending";
        return View();
    }

    public IActionResult PaymentSuccess()
    {
        ViewBag.OrderNumber = OrderNumber;
        ViewBag.Amount = Amount;
        ViewBag.Status = "Paid";
        return View();
    }

    public IActionResult PaymentFailed()
    {
        ViewBag.OrderNumber = OrderNumber;
        ViewBag.Amount = Amount;
        ViewBag.Status = "Failed";
        return View();
    }
}