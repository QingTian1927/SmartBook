namespace SmartBook.Core.DTOs;

public class TopBookDTO
{
    public int BookId { get; set; }
    public string Title { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public int ReaderCount { get; set; }
    public double AverageRating { get; set; }
}