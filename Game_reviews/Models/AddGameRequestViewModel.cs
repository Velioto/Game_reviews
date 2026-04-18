using System.ComponentModel.DataAnnotations;

namespace Game_reviews.Models
{
    public class AddGameRequestViewModel
    {
        [Required]
        [Display(Name = "Game Title")]
        public string GameTitle { get; set; }

        [Required]
        [Display(Name = "Game Description")]
        public string GameDescription { get; set; }

        [Display(Name = "Banner Image URL")]
        public string? BannerUrl { get; set; }

        [Required]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; } = DateTime.Today;

        [Display(Name = "Genres")]
        public List<int> SelectedGenreIds { get; set; } = new();
        public List<Genre> AllGenres { get; set; } = new();

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Message to Admin")]
        public string Message { get; set; }
    }
}