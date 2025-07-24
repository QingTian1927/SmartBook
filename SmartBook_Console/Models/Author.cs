using System;
using System.Collections.Generic;

namespace SmartBook_Console.Models;

public partial class Author
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Bio { get; set; }

    public virtual ICollection<AuthorEditRequest> AuthorEditRequests { get; set; } = new List<AuthorEditRequest>();

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
