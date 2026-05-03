using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollabNest.Data;

namespace CollabNest.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search)
        {
            var query = _db.Projects
                .Include(p => p.Owner)
                .Include(p => p.CollabRequests)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Title.Contains(search) ||
                                         p.Description.Contains(search) ||
                                         p.RequiredSkills.Contains(search));

            ViewBag.Search = search;
            var projects = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(projects);
        }
    }
}
