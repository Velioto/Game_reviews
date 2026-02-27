using Game_reviews.Data;
using Game_reviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Game_reviews.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUBLIC: List games
        public async Task<IActionResult> Index()
        {
            var games = await _context.Games
                .Include(g => g.Reviews)
                .ToListAsync();

            return View(games);
        }

        // PUBLIC: Game details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games
                .Include(g => g.GameGenres)
                    .ThenInclude(gg => gg.Genre)
                .Include(g => g.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null) return NotFound();

            return View(game);
        }

        // ADMIN: Create game (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new GameCreateViewModel
            {
                AllGenres = await _context.Genres.ToListAsync()
            };

            return View(viewModel);
        }

        // ADMIN: Create game (POST)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllGenres = await _context.Genres.ToListAsync();
                return View(model);
            }

            var game = new Game
            {
                Title = model.Title,
                Description = model.Description,
                BannerUrl = model.BannerUrl,
                ReleaseDate = model.ReleaseDate
            };

            foreach (var genreId in model.SelectedGenreIds)
            {
                game.GameGenres.Add(new GameGenre
                {
                    GenreId = genreId
                });
            }

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ADMIN: Edit (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games.FindAsync(id);
            if (game == null) return NotFound();

            return View(game);
        }

        // ADMIN: Edit (POST)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,BannerUrl,ReleaseDate")] Game game)
        {
            if (id != game.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Games.Any(e => e.Id == game.Id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(game);
        }

        // ADMIN: Delete (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games
                .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null) return NotFound();

            return View(game);
        }

        // ADMIN: Delete (POST)
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}