using InnoClinic.Core.Common;

namespace Appointments.Domain.Common;

public static class AppointmentErrors
{
    public static Error MedicalStaffNotFound(Guid medicalStaffId) => new(
        "Appointment.MedicalStaffNotFound",
        $"Medical staff with ID {medicalStaffId} was not found or has no schedule.",
        ErrorType.NotFound);

    public static Error PatientNotFound(Guid patientId) => new(
        "Appointment.PatientNotFound",
        $"Patient with ID {patientId} was not found.",
        ErrorType.NotFound);

    public static Error NotFound(Guid appointmentId) => new(
        "Appointment.NotFound",
        $"Appointment with ID {appointmentId} was not found.",
        ErrorType.NotFound);

    public static readonly Error CannotCancel = new(
        "Appointment.CannotCancel",
        "The appointment cannot be cancelled in its current state.",
        ErrorType.Validation);

    public static readonly Error CannotConfirm = new(
        "Appointment.CannotConfirm",
        "Only planned appointments can be confirmed.",
        ErrorType.Validation);

    public static readonly Error ResultAlreadyExists = new(
        "Appointment.ResultAlreadyExists",
        "A result already exists for this appointment.",
        ErrorType.Conflict);

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
