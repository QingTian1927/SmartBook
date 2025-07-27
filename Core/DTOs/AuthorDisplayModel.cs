namespace SmartBook.Core.DTOs;

public class AuthorDisplayModel
{
    public int Id { get; set; }    
    public string Name { get; set; }
    public string? Bio { get; set; }
    public int BookCount { get; set; } = 0;
}