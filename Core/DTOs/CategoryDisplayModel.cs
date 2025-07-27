namespace SmartBook.Core.DTOs;

public class CategoryDisplayModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public int BookCount { get; set; } = 0;
}