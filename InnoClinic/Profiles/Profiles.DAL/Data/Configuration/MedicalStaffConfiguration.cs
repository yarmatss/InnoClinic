using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.DAL.Entities;

namespace Profiles.DAL.Data.Configuration;

public sealed class MedicalStaffConfiguration : IEntityTypeConfiguration<MedicalStaff>
{
    public void Configure(EntityTypeBuilder<MedicalStaff> builder)
    {
        builder
            .HasOne(ms => ms.Supervisor)
            .WithMany()
            .HasForeignKey(ms => ms.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
