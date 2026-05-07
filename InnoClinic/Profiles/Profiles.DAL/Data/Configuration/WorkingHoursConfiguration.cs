using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.DAL.Entities;

namespace Profiles.DAL.Data.Configuration;

public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.HasKey(wh => new { wh.MedicalStaffId, wh.DayOfWeek });

        builder.HasOne(wh => wh.MedicalStaff)
               .WithMany(ms => ms.WorkingHours)
               .HasForeignKey(wh => wh.MedicalStaffId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(wh => wh.MedicalStaff.IsActive);
    }
}
