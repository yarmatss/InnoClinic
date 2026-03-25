using Profiles.Domain.Enums;

namespace Profiles.DAL.Entities;

public class MedicalStaff : Person
{
    public StaffType StaffType { get; set; }
    public string LicenseNumber { get; set; } = null!;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? SupervisorId { get; set; }
    public MedicalStaff? Supervisor { get; set; }

    public ICollection<StaffSpecialization> StaffSpecializations { get; set; } = [];
}
