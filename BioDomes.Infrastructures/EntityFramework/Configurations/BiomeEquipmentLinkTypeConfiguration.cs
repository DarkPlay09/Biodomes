using BioDomes.Infrastructures.EntityFramework.Links;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BioDomes.Infrastructures.EntityFramework.Configurations;

public class BiomeEquipmentLinkTypeConfiguration : IEntityTypeConfiguration<BiomeEquipmentLink>
{
    public void Configure(EntityTypeBuilder<BiomeEquipmentLink> builder)
    {
        builder.ToTable("BiomeEquipments");

        builder.HasKey(x => new { x.BiomeId, x.EquipmentId });

        builder.HasOne(x => x.Biome)
            .WithMany(x => x.BiomeEquipmentLinks)
            .HasForeignKey(x => x.BiomeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Equipment)
            .WithMany(x => x.BiomeEquipmentLinks)
            .HasForeignKey(x => x.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
