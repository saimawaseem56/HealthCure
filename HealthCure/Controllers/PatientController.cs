using HealthCure.Data;
using HealthCure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCure.Controllers
{
    public class PatientController : Controller
    {
        private readonly HealthCureDbContext _context;


        public PatientController(HealthCureDbContext context)
        {
            _context = context;
        }



        // Admin Login Check
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminId") != null;
        }





        // Patient List
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");


            var patients = await _context.Patients
                .OrderByDescending(p => p.PatientId)
                .ToListAsync();


            return View(patients);
        }





        // Add Patient GET
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");


            return View();
        }






        // Add Patient POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");



            if (ModelState.IsValid)
            {

                _context.Patients.Add(patient);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Patient added successfully!";


                return RedirectToAction(nameof(Index));
            }



            return View(patient);
        }








        // Edit Patient GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");



            var patient = await _context.Patients
                .FindAsync(id);



            if (patient == null)
                return NotFound();



            return View(patient);
        }







        // Edit Patient POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Patient patient)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");



            if (ModelState.IsValid)
            {

                _context.Patients.Update(patient);


                await _context.SaveChangesAsync();



                TempData["Success"] =
                    "Patient updated successfully!";



                return RedirectToAction(nameof(Index));
            }



            return View(patient);
        }








        // Patient Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");



            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);



            if (patient == null)
                return NotFound();



            return View(patient);
        }









        // Delete Patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");



            var patient = await _context.Patients
                .FindAsync(id);



            if (patient == null)
                return NotFound();





            // Appointment check
            bool hasAppointment = await _context.Appointments
                .AnyAsync(a => a.PatientId == id);



            if (hasAppointment)
            {

                TempData["Error"] =
                    "Patient has appointment history and cannot be deleted.";


                return RedirectToAction(nameof(Index));

            }





            _context.Patients.Remove(patient);


            await _context.SaveChangesAsync();



            TempData["Success"] =
                "Patient deleted successfully!";



            return RedirectToAction(nameof(Index));
        }

    }
}