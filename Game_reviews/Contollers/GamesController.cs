using Game_reviews.Data;
using Game_reviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Game_reviews.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GamesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // PUBLIC: List games (with optional genre filter)
        public async Task<IActionResult> Index(int[] selectedGenres, string? searchQuery)
        {
            var gamesQuery = _context.Games
                .Include(g => g.Reviews)
                .Include(g => g.GameGenres)
                    .ThenInclude(gg => gg.Genre)
                .AsQueryable();

            if (selectedGenres != null && selectedGenres.Any())
            {
                gamesQuery = gamesQuery.Where(g =>
                    g.GameGenres.Any(gg => selectedGenres.Contains(gg.GenreId)));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                gamesQuery = gamesQuery.Where(g =>
                    EF.Functions.Like(g.Title, $"%{searchQuery}%") ||
                    EF.Functions.Like(g.Description, $"%{searchQuery}%"));
            }

            var games = await gamesQuery.ToListAsync();
            var allGenres = await _context.Genres.ToListAsync();

            var userId = User.Identity.IsAuthenticated
                ? _userManager.GetUserId(User)
                : null;

            var ownedGameIds = new HashSet<int>();

            if (userId != null)
            {
                ownedGameIds = (await _context.UserGames
                    .Where(ug => ug.UserId == userId)
                    .Select(ug => ug.GameId)
                    .ToListAsync())
                    .ToHashSet();
            }

            var viewModel = new GamesIndexViewModel
            {
                Games = games,
                AllGenres = allGenres,
                SelectedGenreIds = selectedGenres ?? Array.Empty<int>(),
                SearchQuery = searchQuery,
                OwnedGameIds = ownedGameIds
            };

            return View(viewModel);
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

            bool isOwned = false;

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                isOwned = await _context.UserGames
                    .AnyAsync(ug => ug.UserId == userId && ug.GameId == game.Id);
            }

            ViewBag.IsOwned = isOwned;

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

        // USER: Buy game
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Buy(int gameId)
        {
            var userId = _userManager.GetUserId(User);

            var exists = await _context.UserGames
                .AnyAsync(x => x.UserId == userId && x.GameId == gameId);

            if (!exists)
            {
                _context.UserGames.Add(new UserGame
                {
                    UserId = userId,
                    GameId = gameId
                });

                await _context.SaveChangesAsync();

                var gameTitle = await _context.Games
                    .Where(g => g.Id == gameId)
                    .Select(g => g.Title)
                    .FirstOrDefaultAsync();

                TempData["SuccessMessage"] = $"Success! You can now view \"{gameTitle}\" in your library!";
            }

            return RedirectToAction("Details", new { id = gameId });
        }

        // USER: View library (owned games)
        [Authorize]
        public async Task<IActionResult> Library(int[] selectedGenres, string? searchQuery)
        {
            var userId = _userManager.GetUserId(User);

            var gamesQuery = _context.UserGames
                .Where(ug => ug.UserId == userId)
                .Include(ug => ug.Game)
                    .ThenInclude(g => g.Reviews)
                .Include(ug => ug.Game)
                    .ThenInclude(g => g.GameGenres)
                        .ThenInclude(gg => gg.Genre)
                .Select(ug => ug.Game)
                .AsQueryable();

            if (selectedGenres != null && selectedGenres.Any())
            {
                gamesQuery = gamesQuery.Where(g =>
                    g.GameGenres.Any(gg => selectedGenres.Contains(gg.GenreId)));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                gamesQuery = gamesQuery.Where(g =>
                    EF.Functions.Like(g.Title, $"%{searchQuery}%") ||
                    EF.Functions.Like(g.Description ?? "", $"%{searchQuery}%"));
            }

            var games = await gamesQuery.ToListAsync();
            var allGenres = await _context.Genres.ToListAsync();

            var viewModel = new GamesIndexViewModel
            {
                Games = games,
                AllGenres = allGenres,
                SelectedGenreIds = selectedGenres ?? Array.Empty<int>(),
                SearchQuery = searchQuery,
                OwnedGameIds = games.Select(g => g.Id).ToHashSet()
            };

            return View(viewModel);
        }
    }
}