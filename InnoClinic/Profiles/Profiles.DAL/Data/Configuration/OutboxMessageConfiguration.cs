using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.DAL.Entities;

namespace Profiles.DAL.Data.Configuration;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(255);

        builder.Property(x => x.Content)
            .HasColumnType("jsonb");

        builder.Property(x => x.Error)
            .HasMaxLength(2000);
    }
}
