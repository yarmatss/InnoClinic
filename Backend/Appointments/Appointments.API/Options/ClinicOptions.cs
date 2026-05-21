namespace Appointments.API.Options;

public class ClinicOptions
{
    public const string SectionName = "ClinicOptions";

    public string TimeZone { get; init; } = "UTC";
}
