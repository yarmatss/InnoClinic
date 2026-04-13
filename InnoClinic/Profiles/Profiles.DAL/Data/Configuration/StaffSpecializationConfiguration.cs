using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.DAL.Entities;

namespace Profiles.DAL.Data.Configuration;

public sealed class StaffSpecializationConfiguration : IEntityTypeConfiguration<StaffSpecialization>
{
    public void Configure(EntityTypeBuilder<StaffSpecialization> builder)
    {
        builder.HasKey(ss => new { ss.StaffId, ss.SpecializationId });

        builder
            .HasOne(ss => ss.MedicalStaff)
            .WithMany(s => s.StaffSpecializations)
            .HasForeignKey(ss => ss.StaffId);

        builder
            .HasOne(ss => ss.Specialization)
            .WithMany(sp => sp.StaffSpecializations)
            .HasForeignKey(ss => ss.SpecializationId);

        builder
            .HasIndex(ss => new { ss.StaffId, ss.IsPrimary })
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true");

        builder.HasQueryFilter(ss => ss.MedicalStaff.IsActive);
    }
}
