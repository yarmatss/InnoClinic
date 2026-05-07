namespace Profiles.DAL.Entities;

public class ScheduleOverride
{
    public Guid MedicalStaffId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsDayOff { get; set; }

    public MedicalStaff MedicalStaff { get; set; } = null!;
}
