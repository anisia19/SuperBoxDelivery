using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
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
                    IsPersistent = true
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
        public IActionResult SignUp(string username, string password, string confirmPassword, string email)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View();
            }
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
            var superBoxes = _context.SuperBoxes.ToList();
            ViewBag.SuperBoxOptions = new SelectList(superBoxes, "Id", "DisplayAddress");
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            var order = new Order
            {
                SuperBoxId = ""
            };

            return View(order);
        }
  [HttpPost]
  public IActionResult UserHomePage(Order order)
  {
      ViewBag.SuperBoxOptions = new SelectList(_context.SuperBoxes.ToList(), "Id", "DisplayAddress");
      var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
      if (user == null)
      {
          _logger.LogWarning("Logged-in user not found in the database.");
          ModelState.AddModelError("User", "User is not logged in or does not exist.");
      }
      else
      {
          order.User = user;
          order.UserId = user.Id;
      }

      var selectedSuperBox = _context.SuperBoxes.FirstOrDefault(s => s.Id == order.SuperBoxId);
      if (selectedSuperBox == null)
      {
          _logger.LogWarning("Invalid SuperBox selection.");
          ModelState.AddModelError("SuperBoxId", "Please select a valid SuperBox.");
      }
      else
      {
          order.SuperBox = selectedSuperBox;
          order.SuperBoxId = selectedSuperBox.Id;
      }

      if (ModelState.IsValid)
      {
          try
          {
              _context.Orders.Add(order);
              _context.SaveChanges();

              _logger.LogInformation("Order saved successfully.");
              TempData["SuccessMessage"] = "Order placed successfully!";
              return RedirectToAction("UserHomePage");
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Error saving the order.");
              ModelState.AddModelError(string.Empty, "An error occurred while saving your order. Please try again.");
          }
      }
      foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
      {
          _logger.LogWarning("ModelState Error: {ErrorMessage}", error.ErrorMessage);
      }

      return View(order);
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