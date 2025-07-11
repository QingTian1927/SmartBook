using System;
using System.Collections.Generic;

namespace SmartBook.Core.Models;

public partial class CategoryEditRequest
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string? ProposedName { get; set; }

    public int RequestedByUserId { get; set; }

    public DateTime RequestedAt { get; set; }

    public string Status { get; set; } = null!;

    public int? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewComment { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual User RequestedByUser { get; set; } = null!;

    public virtual User? ReviewedByUser { get; set; }
}
