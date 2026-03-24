namespace Profiles.Domain.Entities;

public class Person
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty; // Passport / PESEL
    public string? ContactPhone { get; set; }

    public Patient? PatientProfile { get; set; }
    public MedicalStaff? StaffProfile { get; set; }
}