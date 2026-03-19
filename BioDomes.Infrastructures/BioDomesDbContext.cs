using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.EntityFramework;

public class BioDomesDbContext : DbContext
{
    public BioDomesDbContext(DbContextOptions<BioDomesDbContext> options)
        : base(options)
    {
    }

    public DbSet<SpeciesEntity> Species => Set<SpeciesEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SpeciesEntity>().ToTable("Species");

        // seed ici plus tard

        base.OnModelCreating(modelBuilder);
    }
}