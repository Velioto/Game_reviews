using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Game_reviews.Models
{
    public enum AddGameRequestStatus
    {
        Pending,
        Approved,
        Denied
    }

    public class AddGameRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public IdentityUser? User { get; set; }

        [Required]
        [MaxLength(200)]
        public string GameTitle { get; set; }

        [Required]
        public string GameDescription { get; set; }

        public string? BannerUrl { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string? SelectedGenreIds { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        public AddGameRequestStatus Status { get; set; } = AddGameRequestStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public string? AdminNote { get; set; }
    }
}