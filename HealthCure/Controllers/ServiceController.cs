using HealthCure.Data;
using HealthCure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCure.Controllers
{
    public class ServiceController : Controller
    {
        private readonly HealthCureDbContext _context;

        public ServiceController(HealthCureDbContext context)
        {
            _context = context;
        }

        // Services List
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            var services = await _context.Services
                .Where(s => s.IsActive == true)
                .ToListAsync();

            return View(services);
        }

        // Add Service - GET
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            return View();
        }

        // Add Service - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            if (!ModelState.IsValid)
                return View(service);

            service.IsActive = true;

            _context.Services.Add(service);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Service added successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Edit Service - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            var service = await _context.Services.FindAsync(id);

            if (service == null)
                return NotFound();

            return View(service);
        }

        // Edit Service - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service service)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            var existingService = await _context.Services
                .FindAsync(service.ServiceId);

            if (existingService == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(service);

            existingService.Title = service.Title;
            existingService.Description = service.Description;
            existingService.Icon = service.Icon;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Service updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Delete Service - Soft Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            var service = await _context.Services.FindAsync(id);

            if (service == null)
                return NotFound();

            service.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Service deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}