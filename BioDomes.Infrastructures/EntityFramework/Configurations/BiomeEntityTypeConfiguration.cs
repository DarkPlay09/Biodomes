using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BioDomes.Infrastructures.EntityFramework.Configurations;

public class BiomeEntityTypeConfiguration : IEntityTypeConfiguration<BiomeEntity>
{
    public void Configure(EntityTypeBuilder<BiomeEntity> builder)
    {
        builder.ToTable("Biomes", t =>
        {
            t.HasCheckConstraint("CK_Biomes_Temperature_Range", "[Temperature] >= -100");
            t.HasCheckConstraint("CK_Biomes_AbsoluteHumidity_NonNegative", "[AbsoluteHumidity] >= 0");
            t.HasCheckConstraint("CK_Biomes_State_Enum", "[State] IN ('Optimal','Instable','Critique')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);
        
        builder.Property(x => x.Temperature)
            .IsRequired();
        
        builder.Property(x => x.AbsoluteHumidity)
            .IsRequired();
        
        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(30);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.Creator)
            .WithMany(x => x.CreatedBiomes)
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Restrict); // interdit la suppression d'un User tant qu'il est lié à un biome

        builder.HasIndex(x => new { x.CreatorId, x.Name })
            .IsUnique();
    }
}