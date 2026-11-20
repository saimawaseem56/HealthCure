
using HealthCure.Data;
using HealthCure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCure.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly HealthCureDbContext _context;

        public DepartmentController(HealthCureDbContext context)
        {
            _context = context;
        }

        // Department List
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            var departments = await _context.Departments
                .AsNoTracking()
                .ToListAsync();

            return View(departments);
        }

        // Add Department - GET
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            return View();
        }

        // Add Department - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            if (!ModelState.IsValid)
                return View(department);

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Department added successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Edit Department - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return NotFound();

            return View(department);
        }

        // Edit Department - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department department)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            var existingDepartment = await _context.Departments
                .FindAsync(department.DepartmentId);

            if (existingDepartment == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(department);

            existingDepartment.Name = department.Name;
            existingDepartment.Description = department.Description;
            existingDepartment.ImageUrl = department.ImageUrl;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Department updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Delete Department - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "Admin");

            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return NotFound();

            // Check whether doctors belong to this department
            bool hasDoctors = await _context.Doctors
                .AnyAsync(d => d.DepartmentId == id);

            if (hasDoctors)
            {
                TempData["Error"] =
                    "This department has doctors. Reassign them before deleting.";

                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Department deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
