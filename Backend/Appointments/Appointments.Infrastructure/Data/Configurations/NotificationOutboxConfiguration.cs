using InnoClinic.Messaging.Outbox;
using InnoClinic.Messaging.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appointments.Infrastructure.Data.Configurations;

public class NotificationOutboxConfiguration : IEntityTypeConfiguration<NotificationOutbox>
{
    public void Configure(EntityTypeBuilder<NotificationOutbox> builder)
    {
        builder.ToTable(MessagingConstants.DefaultOutboxTableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageType)
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .HasConversion<string>();
    }
}
