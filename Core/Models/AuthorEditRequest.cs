using System;
using System.Collections.Generic;

namespace SmartBook.Core.Models;

public partial class AuthorEditRequest
{
    public int Id { get; set; }

    public int AuthorId { get; set; }

    public string? ProposedName { get; set; }

    public string? ProposedBio { get; set; }

    public int RequestedByUserId { get; set; }

    public DateTime RequestedAt { get; set; }

    public string Status { get; set; } = null!;

    public int? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewComment { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual User RequestedByUser { get; set; } = null!;

    public virtual User? ReviewedByUser { get; set; }
}
