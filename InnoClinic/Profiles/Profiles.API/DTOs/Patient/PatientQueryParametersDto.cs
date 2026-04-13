using Profiles.API.DTOs.Common;

namespace Profiles.API.DTOs.Patient;

public class PatientQueryParametersDto : PaginationQueryParametersDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? BirthDate { get; set; }
}
