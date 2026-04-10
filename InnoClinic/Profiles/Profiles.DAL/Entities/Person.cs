using Profiles.Domain.Enums;

namespace Profiles.DAL.Entities;

public abstract class Person : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public DateOnly BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string NationalId { get; set; } = null!;
    public string? ContactPhone { get; set; }
}
