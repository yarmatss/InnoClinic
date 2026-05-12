namespace Profiles.DAL.Entities;

public class Patient : Person
{
    public string InsuranceNumber { get; set; } = null!;
    public string? EmergencyContact { get; set; }

    public Guid? PrimaryDoctorId { get; set; }
    public MedicalStaff? PrimaryDoctor { get; set; }
}
