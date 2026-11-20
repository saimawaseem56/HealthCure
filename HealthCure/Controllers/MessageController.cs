using HealthCure.Data;
using HealthCure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCure.Controllers
{
    public class MessageController : Controller
    {
        private readonly HealthCureDbContext _context;

        public MessageController(HealthCureDbContext context)
        {
            _context = context;
        }


        // Admin Message List
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();


            return View(messages);
        }



        // Message Details
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(m => m.ContactId == id);


            if (message == null)
            {
                return NotFound();
            }


            // Mark as read
            message.IsRead = true;
            await _context.SaveChangesAsync();


            return View(message);
        }



        // Delete Message
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var message = await _context.ContactMessages
        //        .FindAsync(id);


        //    if (message != null)
        //    {
        //        _context.ContactMessages.Remove(message);
        //        await _context.SaveChangesAsync();
        //    }


        //    return RedirectToAction(nameof(Index));
        //}
        // Delete Message
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            var message = await _context.ContactMessages
                .FindAsync(id);


            if (message != null)
            {
                _context.ContactMessages.Remove(message);
                await _context.SaveChangesAsync();
            }


            return RedirectToAction(nameof(Index));
        }
    }
}