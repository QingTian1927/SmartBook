using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartBook.Core.Models;

[Index("Email", Name = "UQ__Users__A9D105340F76449F", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(512)]
    public string Password { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
}
