using HealthCure.ViewModels;
using HealthCure.Data;
using HealthCure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HealthCure.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly HealthCureDbContext _context;

        public AppointmentController(HealthCureDbContext context)
        {
            _context = context;
        }
        // GET: Appointment
        public IActionResult Index(string search, string status)
        {
            var appointments = _context.Appointments
                .Join(_context.Patients,
                    a => a.PatientId,
                    p => p.PatientId,
                    (a, p) => new
                    {
                        a.AppointmentId,
                        PatientName = p.FullName,
                        a.DoctorId,
                        a.AppointmentDate,
                        a.Status,
                        a.Message
                    })
                .Join(_context.Doctors,
                    ap => ap.DoctorId,
                    d => d.DoctorId,
                    (ap, d) => new AppointmentViewModel
                    {
                        AppointmentId = ap.AppointmentId,
                        PatientName = ap.PatientName,
                        DoctorName = d.FullName,
                        AppointmentDate = ap.AppointmentDate,
                        Status = ap.Status,
                        Message = ap.Message
                    })
                .AsQueryable();


            // Search Patient / Doctor
            if (!string.IsNullOrEmpty(search))
            {
                appointments = appointments.Where(a =>
                    a.PatientName.Contains(search) ||
                    a.DoctorName.Contains(search));
            }


            // Status Filter
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                appointments = appointments.Where(a =>
                    a.Status == status);
            }


            return View(appointments.ToList());
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Where(a => a.AppointmentId == id)
                .Join(_context.Patients,
                    a => a.PatientId,
                    p => p.PatientId,
                    (a, p) => new
                    {
                        a,
                        PatientName = p.FullName
                    })
                .Join(_context.Doctors,
                    ap => ap.a.DoctorId,
                    d => d.DoctorId,
                    (ap, d) => new AppointmentViewModel
                    {
                        AppointmentId = ap.a.AppointmentId,
                        PatientId = ap.a.PatientId,
                        DoctorId = ap.a.DoctorId,
                        PatientName = ap.PatientName,
                        DoctorName = d.FullName,
                        AppointmentDate = ap.a.AppointmentDate,
                        Status = ap.a.Status,
                        Message = ap.a.Message
                    })
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointment/Create
        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();

            return View(
                "~/Views/Appointment/Create.cshtml",
                new Appointment()
            );
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            if (ModelState.IsValid)
            {
                appointment.Status = "Pending";
                appointment.CreatedDate = DateTime.Now;

                _context.Appointments.Add(appointment);
                _context.SaveChanges();

                TempData["Success"] = "Appointment booked successfully!";

                return RedirectToAction(nameof(Create));
            }

            LoadDropdowns();

            return View("~/Views/Appointment/Create.cshtml", appointment);
        }

        // GET: Appointment/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }
        // POST: Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Appointments.Update(appointment);
                _context.SaveChanges();

                TempData["Success"] = "Appointment updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(appointment);
        }
        // Approve Appointmen
        [HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Approved";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment approved successfully.";

            return RedirectToAction(nameof(Index));
        }


        // Reject Appointment
        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment rejected successfully.";

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var appointment = _context.Appointments
                .FirstOrDefault(a => a.AppointmentId == id);


            if (appointment == null)
            {
                return NotFound();
            }


            _context.Appointments.Remove(appointment);

            _context.SaveChanges();


            TempData["Success"] = "Appointment deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
        private void LoadDropdowns()
        {
            ViewBag.Patients = new SelectList(
                _context.Patients.ToList(),
                "PatientId",
                "FullName");

            ViewBag.Doctors = new SelectList(
                _context.Doctors
                    .Where(d => d.IsActive == true)
                    .ToList(),
                "DoctorId",
                "FullName");
        }
    }
}
