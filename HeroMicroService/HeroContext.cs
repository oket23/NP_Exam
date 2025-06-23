using HeroMicroService.Models;
using Microsoft.EntityFrameworkCore;

namespace HeroMicroService;

public class HeroContext : DbContext
{
    public HeroContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HeroMicroServiceDb;Integrated Security=True");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hero>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder.Entity<Hero>()
            .HasIndex(x => x.Name)
            .IsUnique();
    }

    public DbSet<Hero> Heroes { get; set; }
}
