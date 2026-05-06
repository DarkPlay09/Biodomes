using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BioDomes.Infrastructures.EntityFramework.Configurations;

public class SpeciesEntityTypeConfiguration : IEntityTypeConfiguration<SpeciesEntity>
{
    public void Configure(EntityTypeBuilder<SpeciesEntity> builder)
    {
        builder.ToTable("Species", t =>
        {
            t.HasCheckConstraint("CK_Species_Name_NotBlank", "[Name] <> ''");
            t.HasCheckConstraint("CK_Species_AdultSizeAndWeight_Positive",
                "[AdultSize] > 0 AND  [Weight] > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Classification).IsRequired().HasMaxLength(60);
        builder.Property(x => x.Diet).IsRequired().HasMaxLength(60);
        builder.Property(x => x.AdultSize).IsRequired();
        builder.Property(x => x.Weight).IsRequired();
        builder.Property(x => x.ImagePath).HasMaxLength(300);
        builder.Property(x => x.IsPublicAvailable).IsRequired();

        builder.HasOne(x => x.Creator)
            .WithMany(x => x.CreatedSpecies)
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
