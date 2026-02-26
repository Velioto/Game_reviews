using System.ComponentModel.DataAnnotations;

public class Review
{
    public int Id { get; set; }

    [Range(1, 10)]
    public int Rating { get; set; }

    [Required]
    public string Comment { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public int GameId { get; set; }
    public Game? Game { get; set; }

    public string? UserId { get; set; }
}
