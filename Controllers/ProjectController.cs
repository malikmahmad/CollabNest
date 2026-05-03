using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollabNest.Data;
using CollabNest.Models;
using CollabNest.ViewModels;

namespace CollabNest.Controllers
{
    public class ProjectController : Controller
    {
        private readonly AppDbContext _db;
        public ProjectController(AppDbContext db) => _db = db;

        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectVM vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(vm);

            var project = new Project
            {
                Title = vm.Title,
                Description = vm.Description,
                RequiredSkills = vm.RequiredSkills,
                UserId = userId.Value
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Project posted successfully!";
            return RedirectToAction("Details", new { id = project.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _db.Projects
                .Include(p => p.Owner)
                .Include(p => p.CollabRequests)
                    .ThenInclude(r => r.Sender)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.CurrentUserId = userId;

            bool alreadyRequested = false;
            if (userId != null)
                alreadyRequested = await _db.CollabRequests
                    .AnyAsync(r => r.ProjectId == id && r.SenderId == userId);

            ViewBag.AlreadyRequested = alreadyRequested;
            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(SendRequestVM vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var project = await _db.Projects.FindAsync(vm.ProjectId);
            if (project == null) return NotFound();

            if (project.UserId == userId)
            {
                TempData["Error"] = "You cannot join your own project!";
                return RedirectToAction("Details", new { id = vm.ProjectId });
            }

            bool exists = await _db.CollabRequests
                .AnyAsync(r => r.ProjectId == vm.ProjectId && r.SenderId == userId);

            if (exists)
            {
                TempData["Error"] = "You already sent a request for this project!";
                return RedirectToAction("Details", new { id = vm.ProjectId });
            }

            _db.CollabRequests.Add(new CollabRequest
            {
                ProjectId = vm.ProjectId,
                SenderId = userId.Value,
                Message = vm.Message
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Join request sent successfully!";
            return RedirectToAction("Details", new { id = vm.ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRequest(int requestId, string status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var request = await _db.CollabRequests
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.Project!.UserId != userId)
                return Unauthorized();

            request.Status = status;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Request {status.ToLower()}!";
            return RedirectToAction("Details", new { id = request.ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var project = await _db.Projects.FindAsync(id);

            if (project == null || project.UserId != userId)
                return Unauthorized();

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Project deleted.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
