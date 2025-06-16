using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartBook.Core.Models;

public partial class Book
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public int AuthorID { get; set; }

    public int CategoryID { get; set; }

    public int UserID { get; set; }

    public bool IsRead { get; set; }

    public int? Rating { get; set; }

    [ForeignKey("AuthorID")]
    [InverseProperty("Books")]
    public virtual Author Author { get; set; } = null!;

    [ForeignKey("CategoryID")]
    [InverseProperty("Books")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("UserID")]
    [InverseProperty("Books")]
    public virtual User User { get; set; } = null!;
}
