using Appointments.Domain.Entities;
using InnoClinic.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Appointments.Infrastructure.Data;

public class AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentResult> AppointmentResults => Set<AppointmentResult>();
    public DbSet<NotificationOutbox> NotificationOutboxes => Set<NotificationOutbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
