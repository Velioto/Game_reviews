using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Game_reviews.Models
{
    public class UserGame
    {
        public string UserId { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    }
}