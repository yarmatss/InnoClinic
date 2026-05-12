using Appointments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appointments.Infrastructure.Data.Configurations;

public class AppointmentResultConfiguration : IEntityTypeConfiguration<AppointmentResult>
{
    public void Configure(EntityTypeBuilder<AppointmentResult> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Complaints)
            .HasMaxLength(1000);

        builder.Property(x => x.Conclusion)
            .HasMaxLength(1000);

        builder.Property(x => x.Recommendations)
            .HasMaxLength(1000);
    }
}
