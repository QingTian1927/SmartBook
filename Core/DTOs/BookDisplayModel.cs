using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartBook.Core.DTOs;

public class BookDisplayModel : INotifyPropertyChanged
{
    private string? _description;
    
    public int UserBookId { get; set; }
    public int BookId { get; set; }

    public string Title { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string CategoryName { get; set; } = "";

    public bool IsRead { get; set; }
    public int? Rating { get; set; }

    public string? CoverImagePath { get; set; } = "";

    public string? Reason { get; set; } = ""; // 🆕 New property to explain why the book is recommended

    public string? Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public override string ToString()
    {
        return
            $"{nameof(UserBookId)}: {UserBookId}, {nameof(BookId)}: {BookId}, {nameof(Title)}: {Title}, " +
            $"{nameof(AuthorName)}: {AuthorName}, {nameof(CategoryName)}: {CategoryName}, " +
            $"{nameof(IsRead)}: {IsRead}, {nameof(Rating)}: {Rating}, {nameof(CoverImagePath)}: {CoverImagePath}, " +
            $"{nameof(Reason)}: {Reason}" +
            $"{nameof(Description)}: {Description}";
    }
}