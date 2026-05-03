using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollabNest.Data;

namespace CollabNest.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var myProjects = await _db.Projects
                .Include(p => p.CollabRequests)
                    .ThenInclude(r => r.Sender)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var myRequests = await _db.CollabRequests
                .Include(r => r.Project)
                    .ThenInclude(p => p!.Owner)
                .Where(r => r.SenderId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.MyProjects = myProjects;
            ViewBag.MyRequests = myRequests;
            return View();
        }
    }
}
