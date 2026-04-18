using Game_reviews.Data;
using Game_reviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Game_reviews.Controllers
{
    public class AddGameRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AddGameRequestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GAMEDEV: submit a game request (GET)
        [Authorize(Roles = "GameDev")]
        public async Task<IActionResult> AddGame()
        {
            var model = new AddGameRequestViewModel
            {
                AllGenres = await _context.Genres.ToListAsync()
            };
            return View(model);
        }

        // GAMEDEV: submit a game request (POST)
        [Authorize(Roles = "GameDev")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGame(AddGameRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllGenres = await _context.Genres.ToListAsync();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var request = new AddGameRequest
            {
                UserId = userId,
                GameTitle = model.GameTitle,
                GameDescription = model.GameDescription,
                BannerUrl = model.BannerUrl,
                ReleaseDate = model.ReleaseDate,
                SelectedGenreIds = model.SelectedGenreIds != null
                    ? string.Join(",", model.SelectedGenreIds)
                    : null,
                Message = model.Message,
                Status = AddGameRequestStatus.Pending,
                SubmittedAt = DateTime.Now
            };

            _context.AddGameRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your game request has been submitted!";
            return RedirectToAction("MyRequests");
        }

        // GAMEDEV: view own requests
        [Authorize(Roles = "GameDev")]
        public async Task<IActionResult> MyRequests()
        {
            var userId = _userManager.GetUserId(User);
            var requests = await _context.AddGameRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            return View(requests);
        }

        // ADMIN: view all requests
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var requests = await _context.AddGameRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            return View(requests);
        }

        // ADMIN: approve → creates the game
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.AddGameRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = AddGameRequestStatus.Approved;

            var game = new Game
            {
                Title = request.GameTitle,
                Description = request.GameDescription,
                BannerUrl = request.BannerUrl,
                ReleaseDate = request.ReleaseDate
            };

            if (!string.IsNullOrEmpty(request.SelectedGenreIds))
            {
                foreach (var genreId in request.SelectedGenreIds.Split(',').Select(int.Parse))
                    game.GameGenres.Add(new GameGenre { GenreId = genreId });
            }

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{request.GameTitle}\" approved and added to the library!";
            return RedirectToAction("Manage");
        }

        // ADMIN: deny
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int id, string? adminNote)
        {
            var request = await _context.AddGameRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = AddGameRequestStatus.Denied;
            request.AdminNote = adminNote;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Request for \"{request.GameTitle}\" denied.";
            return RedirectToAction("Manage");
        }
    }
}