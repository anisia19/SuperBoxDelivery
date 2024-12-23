using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == username || u.Email == username);

            if (user != null && user.VerifyPassword(password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")

                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Keeps the user logged in across sessions
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                if (user.IsAdmin)
                {
                    return RedirectToAction("AdminHomePage", "Admin");
                }
                return RedirectToAction("UserHomePage", "Account");
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(string username, string password, string email)
        {
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "Password is required.");
                return View();
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == username);
            if (existingUser != null)
            {
                ModelState.AddModelError("UserName", "Username is already taken.");
                return View();
            }

            var existingEmailUser = _context.Users.FirstOrDefault(u => u.Email == email);
            if (existingEmailUser != null)
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                return View();
            }

            var user = new User { UserName = username, Email = email };
            user.SetPassword(password);

            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("LogIn", "Account");
        }

        [Authorize]
        public IActionResult UserHomePage()
        {
            return View();
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
    }
}