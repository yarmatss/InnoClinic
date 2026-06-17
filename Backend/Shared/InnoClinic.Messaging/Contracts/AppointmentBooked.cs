namespace InnoClinic.Messaging.Contracts;

public record AppointmentBooked(
    Guid AppointmentId,
    Guid PatientId,
    Guid MedicalStaffId,
    DateTime StartTime,
    DateTime EndTime);
