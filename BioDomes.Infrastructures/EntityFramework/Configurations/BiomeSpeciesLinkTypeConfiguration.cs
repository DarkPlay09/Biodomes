using BioDomes.Infrastructures.EntityFramework.Links;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BioDomes.Infrastructures.EntityFramework.Configurations;

public class BiomeSpeciesLinkTypeConfiguration : IEntityTypeConfiguration<BiomeSpeciesLink>
{
    public void Configure(EntityTypeBuilder<BiomeSpeciesLink> builder)
    {
        builder.ToTable("BiomeSpecies");

        builder.HasKey(x => new { x.BiomeId, x.SpeciesId });

        builder.Property(x => x.IndividualCount).IsRequired();

        builder.HasOne(x => x.Biome)
            .WithMany(x => x.BiomeSpeciesLinks)
            .HasForeignKey(x => x.BiomeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Species)
            .WithMany(x => x.BiomeSpeciesLinks)
            .HasForeignKey(x => x.SpeciesId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
