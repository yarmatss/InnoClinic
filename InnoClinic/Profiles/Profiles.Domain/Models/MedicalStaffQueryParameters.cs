using Profiles.Domain.Enums;

namespace Profiles.Domain.Models;

public class MedicalStaffQueryParameters : PaginationParameters
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public StaffType? StaffType { get; set; }
    public Guid? SpecializationId { get; set; }
}
