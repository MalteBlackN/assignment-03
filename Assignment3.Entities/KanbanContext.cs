using Microsoft.EntityFrameworkCore;
namespace Assignment3.Entities;

public sealed class KanbanContext : DbContext
{
    public KanbanContext(DbContextOptions<KanbanContext> options) : base(options)
    {
    }

    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Task> Tasks => Set<Task>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>().Property(c => c.Name)
            .HasMaxLength(50);
        modelBuilder.Entity<Tag>()
            .HasIndex(c => c.Name).IsUnique();
        modelBuilder.Entity<Tag>().Property(c => c.Name)
            .IsRequired();



        modelBuilder.Entity<Task>().Property(c => c.Title)
            .HasMaxLength(100);
        modelBuilder.Entity<Task>().Property(c => c.Title)
            .IsRequired();

        modelBuilder.Entity<Task>().Property(c => c.Description)
            .HasMaxLength(2^31);
        modelBuilder.Entity<Task>().Property(c => c.Description)
            .IsRequired(false);

        modelBuilder.Entity<Task>().Property(c => c.State)
            .IsRequired();

            

        modelBuilder.Entity<User>().Property(c => c.Name)
            .HasMaxLength(100);
        modelBuilder.Entity<User>().Property(c => c.Name)
            .IsRequired();
            
        modelBuilder.Entity<User>().Property(c => c.Email)
            .HasMaxLength(100);
        modelBuilder.Entity<User>().Property(c => c.Email)
            .IsRequired();
        modelBuilder.Entity<User>()
            .HasIndex(c => c.Email).IsUnique();

        modelBuilder
        .Entity<Task>()
        .Property(e => e.State)
        .HasConversion(
            v => v.ToString(),
            v => (State)Enum.Parse(typeof(State), v));
    }
}
