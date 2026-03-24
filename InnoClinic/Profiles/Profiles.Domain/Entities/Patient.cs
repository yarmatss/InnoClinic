namespace Profiles.Domain.Entities;

public class Patient
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public string InsuranceNumber { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }

    public Guid? PrimaryDoctorId { get; set; }
    public MedicalStaff? PrimaryDoctor { get; set; }
}
