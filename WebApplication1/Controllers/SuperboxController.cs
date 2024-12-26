using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class SuperBoxController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SuperBoxController(ApplicationDbContext context)
        {
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
            // Verificăm dacă modelul este valid
            if (!ModelState.IsValid)
            {
                // Logare sau afișare erori de validare
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);  // Afișează eroarea în consolă (pentru debugging)
                }

                TempData["ErrorMessage"] = "Eroare la validarea formularului. Te rugăm să corectezi câmpurile.";
                return View("~/Views/Admin/CreateSuperBox.cshtml", superBox);
            }
            /* Ensure that Order is null if not specified
            if (superBox.Order == null)
            {
                superBox.OrderId = null;  // Explicitly set Order to null
            }*/
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
    }
}
