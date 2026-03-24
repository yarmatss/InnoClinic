namespace Profiles.Domain.Entities;

public class StaffSpecialization
{
    public Guid StaffId { get; set; }
    public MedicalStaff MedicalStaff { get; set; } = null!;

    public Guid SpecializationId { get; set; }
    public Specialization Specialization { get; set; } = null!;

    public bool IsPrimary { get; set; }
    public DateTime CertificationDate { get; set; }
}
