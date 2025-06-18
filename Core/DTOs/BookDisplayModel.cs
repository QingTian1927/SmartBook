namespace SmartBook.Core.DTOs;

public class BookDisplayModel
{
    public int UserBookId { get; set; }
    public int BookId { get; set; }

    public string Title { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string CategoryName { get; set; } = "";

    public bool IsRead { get; set; }
    public int? Rating { get; set; }

    public string? CoverImagePath { get; set; } = ""; 
}