using Game_reviews.Data;
using Game_reviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Game_reviews.Controllers
{
    public class GameDevRoleRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GameDevRoleRequestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ADMIN: view all role requests
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var requests = await _context.GameDevRoleRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            return View(requests);
        }

        // ADMIN: approve → gives user the GameDev role
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.GameDevRoleRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            request.Status = GameDevRoleRequestStatus.Approved;

            await _userManager.AddToRoleAsync(request.User, "GameDev");

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{request.User.Email} has been approved as a Game Dev!";
            return RedirectToAction("Manage");
        }

        // ADMIN: deny
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int id, string? adminNote)
        {
            var request = await _context.GameDevRoleRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = GameDevRoleRequestStatus.Denied;
            request.AdminNote = adminNote;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Request denied.";
            return RedirectToAction("Manage");
        }

        // USER: submit a request to become a GameDev
        [Authorize]
        public IActionResult Submit()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string message)
        {
            var userId = _userManager.GetUserId(User);

            // check if they already have a pending request
            var existing = await _context.GameDevRoleRequests
                .FirstOrDefaultAsync(r => r.UserId == userId && r.Status == GameDevRoleRequestStatus.Pending);

            if (existing != null)
            {
                TempData["ErrorMessage"] = "You already have a pending request.";
                return RedirectToAction("MyRequest");
            }

            var request = new GameDevRoleRequest
            {
                UserId = userId,
                Message = message,
                Status = GameDevRoleRequestStatus.Pending,
                SubmittedAt = DateTime.Now
            };

            _context.GameDevRoleRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your request has been submitted!";
            return RedirectToAction("MyRequest");
        }

        // USER: see their own request status
        [Authorize]
        public async Task<IActionResult> MyRequest()
        {
            var userId = _userManager.GetUserId(User);

            var request = await _context.GameDevRoleRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.SubmittedAt)
                .FirstOrDefaultAsync();

            return View(request);
        }
    }
}