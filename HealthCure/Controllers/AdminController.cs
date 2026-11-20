using HealthCure.Data;
using HealthCure.Models;
using HealthCure.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace HealthCure.Controllers
{
    public class AdminController : Controller
    {
        private readonly HealthCureDbContext _context;

        public AdminController(HealthCureDbContext context)
        {
            _context = context;
        }


        // Admin Login GET
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }


        // Admin Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var admin = await _context.Admins
                .FirstOrDefaultAsync(a =>
                    a.Username == model.Username &&
                    a.IsActive == true);


            if (admin == null || string.IsNullOrEmpty(admin.Password))
            {
                ViewBag.Error = "Invalid Username or Password!";
                return View(model);
            }


            var passwordHasher = new PasswordHasher<Admin>();

            bool isValid = false;


            // First login - plain password ko hash karna
            if (admin.Password == model.Password)
            {
                admin.Password = passwordHasher.HashPassword(
                    admin,
                    model.Password
                );

                await _context.SaveChangesAsync();

                isValid = true;
            }
            else
            {
                var result = passwordHasher.VerifyHashedPassword(
                    admin,
                    admin.Password,
                    model.Password
                );

                isValid = result != PasswordVerificationResult.Failed;
            }


            if (!isValid)
            {
                ViewBag.Error = "Invalid Username or Password!";
                return View(model);
            }


            HttpContext.Session.SetInt32(
                "AdminId",
                admin.AdminId
            );


            HttpContext.Session.SetString(
                "AdminName",
                admin.FullName ?? admin.Username
            );


            return RedirectToAction("Dashboard");
        }



        // Admin Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login");
            }


            ViewBag.TotalDoctors =
                await _context.Doctors.CountAsync();


            ViewBag.TotalPatients =
                await _context.Patients.CountAsync();


            ViewBag.TotalAppointments =
                await _context.Appointments.CountAsync();

            ViewBag.PendingAppointments =
          await _context.Appointments
          .CountAsync(a => a.Status == "Pending");


            ViewBag.ApprovedAppointments =
                await _context.Appointments
                .CountAsync(a => a.Status == "Approved");


            ViewBag.RejectedAppointments =
                await _context.Appointments
                .CountAsync(a => a.Status == "Rejected");


            ViewBag.TotalDepartments =
                await _context.Departments.CountAsync();


            ViewBag.TotalMessages =
                await _context.ContactMessages.CountAsync();



            var recentAppointments = await (
                from a in _context.Appointments
                join p in _context.Patients
                    on a.PatientId equals p.PatientId
                join d in _context.Doctors
                    on a.DoctorId equals d.DoctorId

                orderby a.CreatedDate descending

                select new AppointmentViewModel
                {
                    AppointmentId = a.AppointmentId,

                    PatientName = p.FullName,

                    DoctorName = d.FullName,

                    AppointmentDate = a.AppointmentDate,

                    Status = a.Status,

                    Message = a.Message
                }

            )
            .Take(10)
            .ToListAsync();



            return View(recentAppointments);
        }



        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}