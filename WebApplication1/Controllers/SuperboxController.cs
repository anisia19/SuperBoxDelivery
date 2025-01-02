using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class SuperBoxController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public SuperBoxController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Create()
        {
            return View("~/Views/Admin/CreateSuperBox.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SuperBox superBox)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                TempData["ErrorMessage"] = "Eroare la validarea formularului. Te rugăm să corectezi câmpurile.";
                return View("~/Views/Admin/CreateSuperBox.cshtml", superBox);
            }

            var existingSuperBox = _context.SuperBoxes
                .FirstOrDefault(sb => sb.StreetName == superBox.StreetName
                                      && sb.StreetNumber == superBox.StreetNumber
                                      && sb.ZipCode == superBox.ZipCode
                                      && sb.City == superBox.City);

            if (existingSuperBox != null)
            {
                ModelState.AddModelError("Address", "A SuperBox with this address already exists.");
                return View("~/Views/Admin/CreateSuperBox.cshtml", superBox);
            }

            try
            {
                superBox.Id = Guid.NewGuid().ToString();
                _context.SuperBoxes.Add(superBox);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Superbox created successfully!";
                return RedirectToAction("ViewSuperBox");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "There was an error while saving: " + ex.Message;
                return View("~/Views/Admin/CreateSuperBox.cshtml", superBox);
            }
        }

        public IActionResult ViewSuperBox()
        {
            var superBoxes = _context.SuperBoxes.ToList();
            return View("~/Views/Admin/ViewSuperBox.cshtml", superBoxes);
        }

        public IActionResult ChooseSuperBox()
        {
            var superBoxes = _context.SuperBoxes.ToList();
            return View("~/Views/Admin/ChooseSuperBox.cshtml",
                superBoxes);
        }

        public IActionResult ViewOrdersForSuperBox(string superBoxId)
        {
            var superBox = _context.SuperBoxes
                .FirstOrDefault(s => s.Id == superBoxId);

            if (superBox == null)
            {
                return NotFound();
            }

            var orders = _context.Orders
                .Where(o => o.SuperBoxId == superBoxId)
                .ToList();
            var model = new SuperBoxOrdersViewModel
            {
                SuperBox = superBox,
                Orders = orders
            };

            return View("~/Views/Admin/ViewOrdersForSuperBox.cshtml", model);
        }

        [HttpPost]
        public IActionResult ChangeMultipleOrderStatuses(int[] selectedOrderIds, string superBoxId)
        {
            _logger.LogInformation("Started ChangeMultipleOrderStatuses action.");

            if (selectedOrderIds != null && selectedOrderIds.Length > 0)
            {
                _logger.LogInformation("Selected order IDs: {SelectedOrderIds}", string.Join(", ", selectedOrderIds));

                var ordersToUpdate = _context.Orders
                    .Where(o => selectedOrderIds.Contains(o.OrderId) && o.Status == OrderStatus.InLocker)
                    .ToList();

                foreach (var order in ordersToUpdate)
                {
                    _logger.LogInformation("Updating order ID {OrderId} status to Delivered.", order.OrderId);
                    order.Status = OrderStatus.Delivered;
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Order statuses have been updated.";
            }
            else
            {
                _logger.LogWarning("No orders selected for status update.");
                TempData["ErrorMessage"] = "No orders were selected for updating.";
            }

            var superBox = _context.SuperBoxes
                .FirstOrDefault(s => s.Id == superBoxId);
            if (superBox == null)
            {
                return NotFound();
            }

            var orders = _context.Orders
                .AsQueryable()
                .Where(o => o.SuperBoxId == superBoxId)
                .ToList();

            var model = new SuperBoxOrdersViewModel
            {
                SuperBox = superBox,
                Orders = orders
            };

            return View("~/Views/Admin/ViewOrdersForSuperBox.cshtml", model);
        }
    }
}
