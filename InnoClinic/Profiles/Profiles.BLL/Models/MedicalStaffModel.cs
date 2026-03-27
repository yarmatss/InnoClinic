using Profiles.Domain.Enums;

namespace Profiles.BLL.Models;

public class MedicalStaffModel : PersonModel
{
    public StaffType StaffType { get; set; }
    public string LicenseNumber { get; set; } = null!;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? SupervisorId { get; set; }
}
