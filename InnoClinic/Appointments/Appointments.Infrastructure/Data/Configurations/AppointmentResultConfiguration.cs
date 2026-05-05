using Appointments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appointments.Infrastructure.Data.Configurations;

public class AppointmentResultConfiguration : IEntityTypeConfiguration<AppointmentResult>
{
    public void Configure(EntityTypeBuilder<AppointmentResult> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppointmentId)
            .IsRequired();

        builder.Property(x => x.Complaints)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Conclusion)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Recommendations)
            .IsRequired()
            .HasMaxLength(1000);
    }
}
