using System;
using System.Collections.Generic;

namespace SmartBook.Core.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ICollection<CategoryEditRequest> CategoryEditRequests { get; set; } = new List<CategoryEditRequest>();
}
