using Profiles.Domain.Enums;

namespace Profiles.API.DTOs.Common;

public interface IPersonDto
{
    string FirstName { get; }
    string LastName { get; }
    string? MiddleName { get; }
    Gender Gender { get; }
    string NationalId { get; }
    string? ContactPhone { get; }

    // BirthDate is omitted because medical staff and patients have different requirements
}
