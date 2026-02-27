using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Game_reviews.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Game_reviews.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUBLIC (or you can lock it): GET: Reviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reviews.Include(r => r.Game);
            return View(await applicationDbContext.ToListAsync());
        }

        // PUBLIC (or you can lock it): GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Game)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // USER/Admin: GET: Reviews/Create?gameId=5
        [Authorize]
        public async Task<IActionResult> Create(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) return NotFound();

            ViewBag.GameTitle = game.Title;
            return View(new Review { GameId = game.Id });
        }

        // USER/Admin: POST: Reviews/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Rating,Comment,GameId")] Review review)
        {
            // Server-side fields
            review.CreatedOn = DateTime.UtcNow;
            review.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If for some reason UserId is null even though [Authorize], treat as forbidden
            if (string.IsNullOrEmpty(review.UserId))
                return Forbid();

            if (!ModelState.IsValid)
            {
                var game = await _context.Games.FindAsync(review.GameId);
                if (game == null) return NotFound();

                ViewBag.GameTitle = game.Title;
                return View(review);
            }

            _context.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Games", new { id = review.GameId });
        }

        // OPTIONAL: Admin only edit (recommended)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", review.GameId);
            return View(review);
        }

        // OPTIONAL: Admin only edit (recommended)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rating,Comment,CreatedOn,GameId,UserId")] Review review)
        {
            if (id != review.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", review.GameId);
            return View(review);
        }

        // USER/Admin: GET: Reviews/Delete/5 (owner OR admin)
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Game)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            // Admin can delete anything
            if (User.IsInRole("Admin"))
                return View(review);

            // User can delete only their own review
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.UserId != userId)
                return Forbid();

            return View(review);
        }

        // USER/Admin: POST: Reviews/Delete/5 (owner OR admin)
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (review.UserId != userId)
                    return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Games", new { id = review.GameId });
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}