using Profiles.API.DTOs.Common;
using Profiles.Domain.Enums;

namespace Profiles.API.DTOs.MedicalStaff;

public class MedicalStaffQueryParametersDto : PaginationQueryParametersDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public StaffType? StaffType { get; set; }
    public Guid? SpecializationId { get; set; }
}
