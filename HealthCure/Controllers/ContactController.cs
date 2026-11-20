using HealthCure.Data;
using HealthCure.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthCure.Controllers
{
    public class ContactController : Controller
    {
        private readonly HealthCureDbContext _context;

        public ContactController(HealthCureDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public IActionResult Send(ContactMessage contactmessage)
        {
            if (ModelState.IsValid)
            {
                contactmessage.CreatedDate = DateTime.Now;
                contactmessage.IsRead = false;

                _context.ContactMessages.Add(contactmessage);
                _context.SaveChanges();

                TempData["Success"] = "Your message has been sent successfully!";

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}