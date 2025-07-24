using System;
using System.Collections.Generic;

namespace SmartBook_Console.Models;

public partial class UserBook
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public bool IsRead { get; set; }

    public int? Rating { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
