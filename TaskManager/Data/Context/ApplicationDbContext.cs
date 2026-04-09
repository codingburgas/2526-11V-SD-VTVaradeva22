using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models.Entities;

namespace TaskManager.Data.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Board> Boards => Set<Board>();

    public DbSet<BoardList> BoardLists => Set<BoardList>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Board>()
            .HasOne(b => b.Owner)
            .WithMany(u => u.Boards)
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BoardList>()
            .HasOne(l => l.Board)
            .WithMany(b => b.Lists)
            .HasForeignKey(l => l.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TaskItem>()
            .HasOne(t => t.Board)
            .WithMany(b => b.Tasks)
            .HasForeignKey(t => t.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TaskItem>()
            .HasOne(t => t.BoardList)
            .WithMany(l => l.Tasks)
            .HasForeignKey(t => t.BoardListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TaskItem>()
            .HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Board>()
            .HasIndex(b => new { b.OwnerId, b.Name });

        builder.Entity<BoardList>()
            .HasIndex(l => new { l.BoardId, l.Position });

        builder.Entity<TaskItem>()
            .HasIndex(t => new { t.BoardId, t.Status, t.Priority });

        builder.Entity<TaskItem>()
            .HasIndex(t => t.Deadline);
    }
}
