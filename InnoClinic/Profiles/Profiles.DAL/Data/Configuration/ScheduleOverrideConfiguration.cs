using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.DAL.Entities;

namespace Profiles.DAL.Data.Configuration;

public class ScheduleOverrideConfiguration : IEntityTypeConfiguration<ScheduleOverride>
{
    public void Configure(EntityTypeBuilder<ScheduleOverride> builder)
    {
        builder.HasKey(so => new { so.MedicalStaffId, so.Date });

        builder.HasOne(so => so.MedicalStaff)
               .WithMany(ms => ms.ScheduleOverrides)
               .HasForeignKey(so => so.MedicalStaffId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(so => so.MedicalStaff.IsActive);
    }
}
