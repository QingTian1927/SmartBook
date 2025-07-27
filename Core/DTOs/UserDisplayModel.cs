namespace SmartBook.Core.DTOs;

public class UserDisplayModel
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsBanned { get; set; }
    public int UserBooksCount { get; set; }
}