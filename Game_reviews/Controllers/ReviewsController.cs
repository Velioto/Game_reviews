using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Game_reviews.Data;

namespace Game_reviews.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reviews.Include(r => r.Game);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Game)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // GET: Reviews/Create 
        public async Task<IActionResult> Create(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) return NotFound();

            ViewBag.GameTitle = game.Title;

            return View(new Review { GameId = game.Id });
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Rating,Comment,GameId")] Review review)
        {
            review.CreatedOn = DateTime.UtcNow;

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

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {   
            if (id == null) return NotFound();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", review.GameId);
            return View(review);
        }

        // POST: Reviews/Edit/5
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

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Game)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}