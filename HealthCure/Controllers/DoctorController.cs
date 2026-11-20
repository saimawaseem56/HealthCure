
using HealthCure.Data;
using HealthCure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HealthCure.Controllers
{
    public class DoctorController : Controller
    {
        private readonly HealthCureDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DoctorController(
            HealthCureDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Check Admin Login
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminId") != null;
        }

        // Doctors List
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .Where(d => d.IsActive == true)
                .ToListAsync();

            return View(doctors);
        }

        // Add Doctor - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            await LoadDepartments();

            return View();
        }

        // Add Doctor - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Doctor doctor, IFormFile? imageFile)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            if (!await IsValidDepartment(doctor.DepartmentId))
            {
                ModelState.AddModelError(
                    "DepartmentId", "Please select a valid department.");
            }

            ValidateImage(imageFile, required: true);

            if (!ModelState.IsValid)
            {
                await LoadDepartments(doctor.DepartmentId);
                return View(doctor);
            }

            doctor.ImageUrl = await SaveDoctorImage(imageFile!);
            doctor.IsActive = true;
            doctor.Department = null;

            _context.Doctors.Add(doctor);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Doctor added successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Edit Doctor - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound();

            await LoadDepartments(doctor.DepartmentId);

            return View(doctor);
        }

        // Edit Doctor - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Doctor doctor, IFormFile? imageFile)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var existingDoctor = await _context.Doctors
                .FindAsync(doctor.DoctorId);

            if (existingDoctor == null)
                return NotFound();

            if (!await IsValidDepartment(doctor.DepartmentId))
            {
                ModelState.AddModelError(
                    "DepartmentId", "Please select a valid department.");
            }

            ValidateImage(imageFile, required: false);

            if (!ModelState.IsValid)
            {
                await LoadDepartments(doctor.DepartmentId);
                return View(doctor);
            }

            existingDoctor.FullName = doctor.FullName;
            existingDoctor.Specialization = doctor.Specialization;
            existingDoctor.Qualification = doctor.Qualification;
            existingDoctor.Experience = doctor.Experience;
            existingDoctor.Phone = doctor.Phone;
            existingDoctor.Email = doctor.Email;
            existingDoctor.DepartmentId = doctor.DepartmentId;
            existingDoctor.Description = doctor.Description;
            existingDoctor.IsActive = doctor.IsActive;

            string? oldImage = null;

            if (imageFile != null && imageFile.Length > 0)
            {
                oldImage = existingDoctor.ImageUrl;

                existingDoctor.ImageUrl =
                    await SaveDoctorImage(imageFile);
            }

            await _context.SaveChangesAsync();

            // Delete only the previous uploaded image
            if (!string.IsNullOrEmpty(oldImage))
            {
                DeleteDoctorImage(oldImage);
            }

            TempData["Success"] = "Doctor updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Delete Doctor (Soft Delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound();

            // Keep appointment history safe
            doctor.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Doctor removed successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Check Department
        private async Task<bool> IsValidDepartment(int? departmentId)
        {
            int id = Convert.ToInt32(departmentId);

            if (id <= 0)
                return false;

            return await _context.Departments
                .AnyAsync(d => d.DepartmentId == id);
        }

        // Load Departments Dropdown
        private async Task LoadDepartments(int? selectedId = null)
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .ToListAsync();

            ViewBag.DepartmentId = new SelectList(
                departments,
                "DepartmentId",
                "Name",
                selectedId
            );
        }

        // Validate Doctor Image
        private bool ValidateImage(
            IFormFile? imageFile, bool required)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                if (required)
                {
                    ModelState.AddModelError(
                        "ImageUrl", "Please select doctor image.");

                    return false;
                }

                return true;
            }

            var extension = Path.GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "ImageUrl",
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

                return false;
            }

            if (imageFile.Length > 3 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "ImageUrl",
                    "Image size must be below 3 MB.");

                return false;
            }

            return true;
        }

        // Save Doctor Image
        private async Task<string> SaveDoctorImage(IFormFile imageFile)
        {
            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "Uploads",
                "Doctors"
            );

            Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            var fileName = Guid.NewGuid().ToString("N") + extension;

            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(
                filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "Uploads/Doctors/" + fileName;
        }

        // Delete Old Uploaded Image
        private void DeleteDoctorImage(string imageUrl)
        {
            if (!imageUrl.StartsWith(
                "Uploads/Doctors/",
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var uploadRoot = Path.GetFullPath(
                Path.Combine(
                    _environment.WebRootPath,
                    "Uploads",
                    "Doctors"
                )
            );

            var filePath = Path.GetFullPath(
                Path.Combine(
                    _environment.WebRootPath,
                    imageUrl.Replace(
                        '/',
                        Path.DirectorySeparatorChar)
                )
            );

            if (!filePath.StartsWith(
                uploadRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
