using Profiles.Domain.Enums;

namespace Profiles.Domain.Entities;

public class MedicalStaff
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;
    public StaffType StaffType { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? SupervisorId { get; set; }
    public MedicalStaff? Supervisor { get; set; }

    public ICollection<StaffSpecialization> StaffSpecializations { get; set; } = [];
}
