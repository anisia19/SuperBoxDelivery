using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddAdmin(string usernameOrEmail)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail);

            if (user != null)
            {
                user.IsAdmin = true;
                _context.SaveChanges();
                TempData["Message"] = "User promoted to admin!";
                return RedirectToAction("AdminHomePage", "Admin");
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("AdminHomePage", "Admin");
            }
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AdminHomePage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("LogIn", "Account");
        }
        [HttpPost]
        public IActionResult ChangeOrderStatus(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == OrderStatus.Canceled)
            {
                return BadRequest("The order is already canceled and cannot be delivered.");
            }
            if (order.Status != OrderStatus.Delivered)
            {
                order.Status = OrderStatus.Delivered;
                _context.SaveChanges();
            }
            return RedirectToAction("ViewAllOrders", "Account");
        }
    }
}
