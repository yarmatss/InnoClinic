namespace Appointments.Domain.Constants;

public static class CacheConstants
{
    public static string MedicalStaffScheduleKey(Guid id) => $"medicalstaff:schedule:{id}";
    public static string MedicalStaffScheduleKey(string id) => $"medicalstaff:schedule:{id}";
}
