namespace SmartBook.Core.DTOs;

public class TopUserDTO
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public int ReadBooksCount { get; set; }
}