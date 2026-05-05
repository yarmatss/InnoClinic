using Appointments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appointments.Infrastructure.Data.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DoctorId)
            .IsRequired();

        builder.Property(x => x.Start)
            .IsRequired();

        builder.Property(x => x.End)
            .IsRequired();

        builder.Property(x => x.AppointmentId);
    }
}
