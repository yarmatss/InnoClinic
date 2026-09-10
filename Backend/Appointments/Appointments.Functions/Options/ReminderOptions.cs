namespace Appointments.Functions.Options;

public class ReminderOptions
{
    public const string SectionName = "ReminderOptions";

    public string Schedule { get; set; } = "0 */15 * * * *";
    public int LeadTimeHours { get; set; } = 24;
    public int BatchSize { get; set; } = 100;
}
