using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartBook.Core.Models;

[Index("Title", "AuthorId", Name = "IX_Books_TitleAuthor", IsUnique = true)]
public partial class Book
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public int AuthorId { get; set; }

    public int CategoryId { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("Books")]
    public virtual Author Author { get; set; } = null!;

    [ForeignKey("CategoryId")]
    [InverseProperty("Books")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("Book")]
    public virtual ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();

    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(Title)}: {Title}, {nameof(AuthorId)}: {AuthorId}, {nameof(CategoryId)}: {CategoryId}, {nameof(Author)}: {Author}, {nameof(Category)}: {Category}, {nameof(UserBooks)}: {UserBooks}";
    }
}
