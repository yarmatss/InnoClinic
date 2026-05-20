using InnoClinic.Core.Common;

namespace Appointments.Domain.Common;

public static class AppointmentErrors
{
    public static Error MedicalStaffNotFound(Guid medicalStaffId) => new(
        "Appointment.MedicalStaffNotFound",
        $"Medical staff with ID {medicalStaffId} was not found or has no schedule.",
        ErrorType.NotFound);

    public static readonly Error OutsideWorkingHours = new(
        "Appointment.OutsideWorkingHours",
        "The requested appointment time is outside the medical staff's working hours.",
        ErrorType.Validation);

    public static readonly Error MedicalStaffOnDayOff = new(
        "Appointment.MedicalStaffOnDayOff",
        "The medical staff is not working on the requested day.",
        ErrorType.Validation);

    public static readonly Error ScheduleConflict = new(
        "Appointment.ScheduleConflict",
        "The requested time slot overlaps with an existing appointment.",
        ErrorType.Conflict);
}
