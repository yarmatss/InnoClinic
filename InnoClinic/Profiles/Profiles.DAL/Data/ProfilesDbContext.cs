using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Entities;
using System.Reflection;

namespace Profiles.DAL.Data;

public class ProfilesDbContext(DbContextOptions<ProfilesDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MedicalStaff> Staff => Set<MedicalStaff>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
    public DbSet<ScheduleOverride> ScheduleOverrides => Set<ScheduleOverride>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
