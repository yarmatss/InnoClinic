using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.DAL.Entities;

namespace Profiles.DAL.Data.Configuration;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder
            .HasOne(p => p.PrimaryDoctor)
            .WithMany()
            .HasForeignKey(p => p.PrimaryDoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
