using HealthCure.Data;
using HealthCure.Models;
using HealthCure.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HealthCure.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HealthCureDbContext _context;

        public HomeController(ILogger<HomeController> logger, HealthCureDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomePageViewModel
            {
                Doctors = await _context.Doctors
                    .Include(d => d.Department)
                    .Where(d => d.IsActive == true)
                    .AsNoTracking()
                    .ToListAsync(),

                Services = await _context.Services
                    .Where(s => s.IsActive == true)
                    .AsNoTracking()
                    .ToListAsync(),
                Departments = await _context.Departments
                    .AsNoTracking()
                    .ToListAsync()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ContactMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var homeModel = new HomePageViewModel
                {
                    Doctors = await _context.Doctors
                        .Include(d => d.Department)
                        .Where(d => d.IsActive == true)
                        .AsNoTracking()
                        .ToListAsync(),

                    Services = await _context.Services
                        .Where(s => s.IsActive == true)
                        .AsNoTracking()
                        .ToListAsync(),

                    Departments = await _context.Departments
                        .AsNoTracking()
                        .ToListAsync(),

                    Contact = model
                };

                return View("Index", homeModel);
            }

            ContactMessage contact = new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Subject = model.Subject,
                Message = model.Message,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            _context.ContactMessages.Add(contact);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your message has been sent successfully!";

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}