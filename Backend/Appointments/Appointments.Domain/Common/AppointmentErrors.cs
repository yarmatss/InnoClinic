using InnoClinic.Core.Common;

namespace Appointments.Domain.Common;

public static class AppointmentErrors
{
    public static Error DoctorNotFound(Guid doctorId) => new(
        "Appointment.DoctorNotFound",
        $"Doctor with ID {doctorId} was not found or has no schedule.",
        ErrorType.NotFound);

    public static readonly Error OutsideWorkingHours = new(
        "Appointment.OutsideWorkingHours",
        "The requested appointment time is outside the doctor's working hours.",
        ErrorType.Validation);

    public static readonly Error DoctorOnDayOff = new(
        "Appointment.DoctorOnDayOff",
        "The doctor is not working on the requested day.",
        ErrorType.Validation);

    public static readonly Error ScheduleConflict = new(
        "Appointment.ScheduleConflict",
        "The requested time slot overlaps with an existing appointment.",
        ErrorType.Conflict);
}
