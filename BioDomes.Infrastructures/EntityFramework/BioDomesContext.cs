using BioDomes.Domains.Entities;
using BioDomes.Infrastructures.EntityFramework.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.EntityFramework;

public class BioDomesContext : DbContext
{
    public BioDomesContext(DbContextOptions options) 
        : base(options)
    {}
    
    // TODO : mettre toutes les tables de la DB ici sous la forme : 
     public DbSet<Species> Species { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SpecieEntityTypeConfiguration());
    }
}
