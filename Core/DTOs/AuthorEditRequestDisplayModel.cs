namespace SmartBook.Core.DTOs;

public class AuthorEditRequestDisplayModel
{
    public int Id { get; set; }
    public string CurrentAuthorName { get; set; } = "";
    public string? CurrentBio { get; set; }
    public string? ProposedName { get; set; }
    public string? ProposedBio { get; set; }
    public string RequestedByUsername { get; set; } = "";
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = "";
    public string? ReviewComment { get; set; }
}