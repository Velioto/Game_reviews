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

       

       

        // USER/Admin: GET: Reviews/Create
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

        // Edit GET
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            // Authorization: check if current user is admin or the review's author
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && review.UserId != currentUserId)
            {
                return Forbid(); // or return Unauthorized() / NotFound() for security through obscurity
            }

            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", review.GameId);
            return View(review);
        }

        // EDIT: POST
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rating,Comment")] Review updatedReview) // removed UserId and CreatedOn from Bind
        {
            if (id != updatedReview.Id) return NotFound();

            // Fetch the original review from the database
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            // Authorization: check again (in case the user tries to edit a different review)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && review.UserId != currentUserId)
            {
                return Forbid();
            }

            // Only update allowed fields
            if (ModelState.IsValid)
            {
                review.Rating = updatedReview.Rating;
                review.Comment = updatedReview.Comment;
                // review.CreatedOn remains unchanged; UserId remains unchanged

                try
                {
                    _context.Update(review); // This will mark only changed properties as modified
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Games", new { id = review.GameId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id)) return NotFound();
                    throw;
                }
            }

            // If ModelState is invalid, repopulate ViewData and return the view with the updatedReview (or original review)
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", review.GameId);
            return View(review); // return the original review with errors, or updatedReview if you prefer
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