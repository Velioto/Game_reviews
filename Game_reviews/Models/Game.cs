public class Game
{
    public int Id { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public string? BannerUrl { get; set; }
    public DateTime ReleaseDate { get; set; }

    public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
