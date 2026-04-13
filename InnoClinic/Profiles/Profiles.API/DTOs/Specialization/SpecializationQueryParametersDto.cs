using Profiles.API.DTOs.Common;

namespace Profiles.API.DTOs.Specialization;

public class SpecializationQueryParametersDto : PaginationQueryParametersDto
{
    public string? Name { get; set; }
}
