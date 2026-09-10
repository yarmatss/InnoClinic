namespace InnoClinic.Messaging.Contracts;

public record AppointmentReminder(
    Guid AppointmentId,
    Guid PatientId,
    Guid MedicalStaffId,

    DateTime StartTime,
    DateTime EndTime,
    
    string PatientEmail,
    string PatientName,
    string MedicalStaffName);
