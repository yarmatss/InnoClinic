namespace Profiles.BLL.Models;

public class WorkingHoursModel
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsDayOff { get; set; }
}
