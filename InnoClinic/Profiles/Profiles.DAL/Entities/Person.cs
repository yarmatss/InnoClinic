namespace Profiles.DAL.Entities;

public abstract class Person : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string Gender { get; set; } = null!;
    public string NationalId { get; set; } = null!; // Passport / PESEL
    public string? ContactPhone { get; set; }
}
