namespace InnoClinic.Core.Authorization;

public record User(string UserId, Guid? PatientId, Guid? StaffId, UserRole Role)
{
    public bool IsPatient => Role == UserRole.Patient;
    public bool IsDoctor => Role == UserRole.Doctor;
    public bool IsReceptionist => Role == UserRole.Receptionist;
    public bool IsAdmin => Role == UserRole.Administrator;
}
