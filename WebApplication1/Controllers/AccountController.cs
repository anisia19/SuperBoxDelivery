﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

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
                ViewBag.SuccessMessage = TempData["SuccessMessage"]?.ToString();
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
            var user = _context.Users.FirstOrDefault(u => User.Identity != null && u.UserName == User.Identity.Name);
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
                var ordersInLocker = _context.Orders
                    .Where(o => o.SuperBoxId == selectedSuperBox.Id && o.Status == OrderStatus.InLocker)
                    .Count();
                if (ordersInLocker >= selectedSuperBox.Capacity)
                {
                    _logger.LogWarning("SuperBox is full. User cannot place an order.");
                    ModelState.AddModelError("SuperBoxId", "This SuperBox is full! Please choose another one.");
                }
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
        [Authorize]
        public IActionResult UserOrders()
        {
            var user = _context.Users.FirstOrDefault(u => User.Identity != null && u.UserName == User.Identity.Name);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return RedirectToAction("UserHomePage");
            }
            var userOrders = _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderId)
                .Include(o => o.SuperBox)
                .ToList();

            return View(userOrders);
        }

        [HttpPost]
        public IActionResult CancelOrder(int[] selectedOrderIds)
        {
            if (selectedOrderIds != null && selectedOrderIds.Length > 0)
            {
                var ordersToUpdate = _context.Orders
                    .Where(o => selectedOrderIds.Contains(o.OrderId) && o.Status == OrderStatus.InLocker)
                    .ToList();

                foreach (var order in ordersToUpdate)
                {
                    _logger.LogInformation("Updating order ID {OrderId} status to Canceled.", order.OrderId);
                    order.Status = OrderStatus.Canceled;
                }
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Orders have been canceled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "No orders were selected to cancel.";
            }
            return RedirectToAction("UserHomePage");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("LogIn", "Account");
        }
        public IActionResult ViewAllOrders()
        {
            var orders = _context.Orders
                .OrderByDescending(o => o.OrderId)
                .ToList();
            return View("ViewAllOrders", orders);
        }

        [HttpGet]
        public IActionResult PlaceOrder()
        {
            ViewBag.SuperBoxOptions = new SelectList(_context.SuperBoxes, "Id", "DisplayAddress");
            return View("PlaceOrder");
        }

        [HttpPost]
        public IActionResult PlaceOrder(PlaceOrderModel model)
        {
            ViewBag.SuperBoxOptions = new SelectList(_context.SuperBoxes.ToList(), "Id", "DisplayAddress");
            var receiverUser = _context.Users.FirstOrDefault(u => u.UserName == model.ReceiverUserName);
            var currentUser = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            var senderSuperBox = _context.SuperBoxes.FirstOrDefault(sb => sb.Id == model.SuperBoxId);
            var receiverSuperBox = _context.SuperBoxes.FirstOrDefault(sb => sb.Id == model.ReceiverSuperBoxId);

            if (receiverUser == null)
            {
                ModelState.AddModelError("ReceiverUserName", "The specified receiver username was not found.");
            }
            else if (currentUser != null && receiverUser.UserName == currentUser.UserName)
            {
                ModelState.AddModelError("ReceiverUserName", "You cannot send a package to yourself.");
            }
            else if (model.SuperBoxId == model.ReceiverSuperBoxId)
            {
                ModelState.AddModelError("ReceiverSuperBoxId", "The sender's SuperBox cannot be the same as the receiver's SuperBox.");
            }
            else if (senderSuperBox == null || receiverSuperBox == null)
            {
                ModelState.AddModelError(string.Empty, "One of the selected SuperBoxes was not found.");
            }
            else if (currentUser != null)
            {
                model.ReceiverUserId = receiverUser.Id;
                model.UserId = currentUser.Id;
            }

            if (senderSuperBox != null)
            {
                var ordersInSenderSuperBox = _context.Orders
                    .Where(o => o.SuperBoxId == senderSuperBox.Id && o.Status == OrderStatus.InLocker)
                    .Count();

                if (ordersInSenderSuperBox >= senderSuperBox.Capacity)
                {
                    _logger.LogWarning("Sender SuperBox is full. User cannot place an order.");
                    ModelState.AddModelError("SuperBoxId", "The sender's SuperBox is full! Please choose another one.");
                }
            }

            if (receiverSuperBox != null)
            {
                var ordersInReceiverSuperBox = _context.Orders
                    .Where(o => o.SuperBoxId == receiverSuperBox.Id && o.Status == OrderStatus.InLocker)
                    .Count();

                if (ordersInReceiverSuperBox >= receiverSuperBox.Capacity)
                {
                    _logger.LogWarning("Receiver SuperBox is full. User cannot place an order.");
                    ModelState.AddModelError("ReceiverSuperBoxId", "The receiver's SuperBox is full! Please choose another one.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var order = new Order
                    {
                        SuperBoxId = model.SuperBoxId,
                        ReceiverUserId = model.ReceiverUserId,
                        ReceiverSuperBoxId = model.ReceiverSuperBoxId,
                        IsUrgent = model.IsUrgent,
                        RelevantInfo = model.RelevantInfo,
                        Status = OrderStatus.InLocker,
                        UserId = model.UserId
                    };

                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    _logger.LogInformation("Order placed successfully.");
                    TempData["SuccessMessage"] = "Your order has been placed successfully!";
                    return RedirectToAction("PlaceOrder");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error placing the order.");
                    ModelState.AddModelError(string.Empty, "An error occurred while placing your order. Please try again.");
                }
            }

            return View(model);
        }


        public IActionResult ReceivingOrdersFromUsers()
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var orders = _context.Orders
                .Where(o => o.ReceiverUserId == user.Id)
                .OrderByDescending(o => o.OrderId)
                .Join(_context.Users,
                    order => order.UserId,
                    u => u.Id,
                    (order, u) => new
                    {
                        order.OrderId,
                        order.SuperBoxId,
                        order.ReceiverUserId,
                        order.ReceiverSuperBoxId,
                        order.IsUrgent,
                        order.RelevantInfo,
                        order.Status,
                        UserName = u.UserName,
                        SuperBoxAddress = order.SuperBox.DisplayAddress,
                        ReceiverSuperBoxAddress = order.ReceiverSuperBox.DisplayAddress
                    })
                .ToList();
            return View("ReceivingOrdersFromUsers", orders);
        }
        [HttpGet]
        public IActionResult MakeOrder()
        {
            ViewBag.SuperBoxOptions = new SelectList(_context.SuperBoxes, "Id", "DisplayAddress");
            return View("MakeOrder");
        }
    }
}