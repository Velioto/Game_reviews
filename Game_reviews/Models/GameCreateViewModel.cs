namespace Game_reviews.Models
{
    public class GameCreateViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? BannerUrl { get; set; }
        public DateTime ReleaseDate { get; set; }

        public List<int> SelectedGenreIds { get; set; } = new();
        public List<Genre> AllGenres { get; set; } = new();
    }
}
