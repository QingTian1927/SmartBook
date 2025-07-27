namespace SmartBook.Core.DTOs;

public class CategoryEditRequestDisplayModel
{
    public int Id { get; set; }
    public string CurrentCategoryName { get; set; }
    public string ProposedName { get; set; }
    public string RequestedByUsername { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; }
    public string ReviewComment { get; set; }
}