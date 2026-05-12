namespace Profiles.BLL.Models;

public class ScheduleOverrideModel
{
    public DateOnly Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsDayOff { get; set; }
}
