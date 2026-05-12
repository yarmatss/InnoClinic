namespace Appointments.Domain.Entities;

public class AppointmentResult : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public string Complaints { get; set; } = null!;
    public string Conclusion { get; set; } = null!;
    public string Recommendations { get; set; } = null!;
}
