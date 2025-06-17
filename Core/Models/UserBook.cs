using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartBook.Core.Models;

[Index("UserId", "BookId", Name = "UX_UserBooks_User_Book", IsUnique = true)]
public partial class UserBook
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public bool IsRead { get; set; }

    public int? Rating { get; set; }

    [ForeignKey("BookId")]
    [InverseProperty("UserBooks")]
    public virtual Book Book { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserBooks")]
    public virtual User User { get; set; } = null!;

    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(UserId)}: {UserId}, {nameof(BookId)}: {BookId}, {nameof(IsRead)}: {IsRead}, {nameof(Rating)}: {Rating}, {nameof(Book)}: {Book}, {nameof(User)}: {User}";
    }
}
