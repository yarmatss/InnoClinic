namespace Appointments.Domain.Entities;

public class TimeSlot : BaseEntity
{
    public Guid MedicalStaffId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Guid? AppointmentId { get; set; }
}
