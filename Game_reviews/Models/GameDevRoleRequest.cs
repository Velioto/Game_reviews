using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Game_reviews.Models
{
    public enum GameDevRoleRequestStatus
    {
        Pending,
        Approved,
        Denied
    }

    public class GameDevRoleRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public IdentityUser? User { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } // why they want to be a GameDev

        public GameDevRoleRequestStatus Status { get; set; } = GameDevRoleRequestStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public string? AdminNote { get; set; }
    }
}