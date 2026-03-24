using Microsoft.EntityFrameworkCore;
using Profiles.Domain.Entities;

namespace Profiles.DAL.Data;

public class ProfilesDbContext(DbContextOptions<ProfilesDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MedicalStaff> Staff => Set<MedicalStaff>();
    public DbSet<Specialization> Specializations => Set<Specialization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>()
            .HasOne(p => p.PatientProfile)
            .WithOne(pt => pt.Person)
            .HasForeignKey<Patient>(pt => pt.PersonId);

        modelBuilder.Entity<Person>()
            .HasOne(p => p.StaffProfile)
            .WithOne(ms => ms.Person)
            .HasForeignKey<MedicalStaff>(ms => ms.PersonId);

        modelBuilder.Entity<StaffSpecialization>()
            .HasKey(ss => new { ss.StaffId, ss.SpecializationId });

        modelBuilder.Entity<StaffSpecialization>()
            .HasOne(ss => ss.MedicalStaff)
            .WithMany(s => s.StaffSpecializations)
            .HasForeignKey(ss => ss.StaffId);

        modelBuilder.Entity<StaffSpecialization>()
            .HasOne(ss => ss.Specialization)
            .WithMany(sp => sp.StaffSpecializations)
            .HasForeignKey(ss => ss.SpecializationId);

        modelBuilder.Entity<StaffSpecialization>()
            .HasIndex(ss => new { ss.StaffId, ss.IsPrimary })
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true");

        modelBuilder.Entity<MedicalStaff>()
            .HasOne(ms => ms.Supervisor)
            .WithMany()
            .HasForeignKey(ms => ms.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Patient>()
            .HasOne(p => p.PrimaryDoctor)
            .WithMany()
            .HasForeignKey(p => p.PrimaryDoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
