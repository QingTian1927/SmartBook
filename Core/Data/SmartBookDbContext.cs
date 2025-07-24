using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartBook.Core.Models;

namespace SmartBook.Core.Data;

public partial class SmartBookDbContext : DbContext
{
    public SmartBookDbContext()
    {
    }

    public SmartBookDbContext(DbContextOptions<SmartBookDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<AuthorEditRequest> AuthorEditRequests { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryEditRequest> CategoryEditRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBook> UserBooks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(config.GetConnectionString("DB"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Authors__3214EC079C542960");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<AuthorEditRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuthorEd__3214EC07B8A2703B");

            entity.HasIndex(e => e.Status, "IX_AuthorEditRequests_Status");

            entity.Property(e => e.ProposedName).HasMaxLength(100);
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ReviewComment).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Author).WithMany(p => p.AuthorEditRequests)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK_AuthorEditRequests_Authors");

            entity.HasOne(d => d.RequestedByUser).WithMany(p => p.AuthorEditRequestRequestedByUsers)
                .HasForeignKey(d => d.RequestedByUserId)
                .HasConstraintName("FK_AuthorEditRequests_Users_Requester");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.AuthorEditRequestReviewedByUsers)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK_AuthorEditRequests_Users_Reviewer");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Books__3214EC073E67B45C");

            entity.HasIndex(e => new { e.Title, e.AuthorId }, "IX_Books_TitleAuthor").IsUnique();

            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Books_Authors");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Books_Categories");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC073BEE0368");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<CategoryEditRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC078AC1CCE0");

            entity.HasIndex(e => e.Status, "IX_CategoryEditRequests_Status");

            entity.Property(e => e.ProposedName).HasMaxLength(100);
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ReviewComment).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryEditRequests)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_CategoryEditRequests_Categories");

            entity.HasOne(d => d.RequestedByUser).WithMany(p => p.CategoryEditRequestRequestedByUsers)
                .HasForeignKey(d => d.RequestedByUserId)
                .HasConstraintName("FK_CategoryEditRequests_Users_Requester");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.CategoryEditRequestReviewedByUsers)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK_CategoryEditRequests_Users_Reviewer");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07B686AE73");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105340F76449F").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsBanned).HasColumnName("is_banned");
            entity.Property(e => e.Password).HasMaxLength(512);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserBook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserBook__3214EC07FF734993");

            entity.HasIndex(e => new { e.UserId, e.BookId }, "UX_UserBooks_User_Book").IsUnique();

            entity.HasOne(d => d.Book).WithMany(p => p.UserBooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserBooks_Books");

            entity.HasOne(d => d.User).WithMany(p => p.UserBooks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserBooks_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
