using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BioDomes.Infrastructures.EntityFramework.Configurations;

public class EquipmentEntityTypeConfiguration : IEntityTypeConfiguration<EquipmentEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentEntity> builder)
    {
        builder.ToTable("Equipments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(120);
        builder.Property(x => x.ImagePath).HasMaxLength(300);
        builder.Property(x => x.ProducedElement).HasMaxLength(100);
        builder.Property(x => x.ConsumedElement).HasMaxLength(100);
        builder.Property(x => x.IsPublicAvailable).IsRequired();

        builder.HasOne(x => x.Creator)
            .WithMany(x => x.CreatedEquipments)
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Name);
    }
}
