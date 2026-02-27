using System.Collections.Generic;

namespace Game_reviews.Models
{
    public class GamesIndexViewModel
    {
        public IEnumerable<Game> Games { get; set; }
        public List<Genre> AllGenres { get; set; }
        public int[] SelectedGenreIds { get; set; }
    }
}