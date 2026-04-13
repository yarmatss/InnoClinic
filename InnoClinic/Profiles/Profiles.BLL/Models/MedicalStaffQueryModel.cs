using Profiles.Domain.Enums;

namespace Profiles.BLL.Models;

public class MedicalStaffQueryModel : BaseQueryModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public StaffType? StaffType { get; set; }
    public Guid? SpecializationId { get; set; }
}
