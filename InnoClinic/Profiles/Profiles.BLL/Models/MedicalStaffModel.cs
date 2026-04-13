using Profiles.Domain.Enums;

namespace Profiles.BLL.Models;

public class MedicalStaffModel : PersonModel
{
    public StaffType StaffType { get; set; }
    public required string LicenseNumber { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? SupervisorId { get; set; }

    public IReadOnlyList<StaffSpecializationModel> StaffSpecializations { get; set; } = [];
}
