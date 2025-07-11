using System;
using System.Collections.Generic;

namespace SmartBook.Core.Models;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int AuthorId { get; set; }

    public int CategoryId { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
}
