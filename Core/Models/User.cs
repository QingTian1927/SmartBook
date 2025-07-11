using System;
using System.Collections.Generic;

namespace SmartBook.Core.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<AuthorEditRequest> AuthorEditRequestRequestedByUsers { get; set; } = new List<AuthorEditRequest>();

    public virtual ICollection<AuthorEditRequest> AuthorEditRequestReviewedByUsers { get; set; } = new List<AuthorEditRequest>();

    public virtual ICollection<CategoryEditRequest> CategoryEditRequestRequestedByUsers { get; set; } = new List<CategoryEditRequest>();

    public virtual ICollection<CategoryEditRequest> CategoryEditRequestReviewedByUsers { get; set; } = new List<CategoryEditRequest>();

    public virtual ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
}
