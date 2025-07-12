namespace SmartBook.Core.DTOs;

public class GeminiRecommendationDTO
{
    public int BookId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
}