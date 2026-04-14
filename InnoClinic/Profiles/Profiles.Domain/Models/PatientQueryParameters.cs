namespace Profiles.Domain.Models;

public class PatientQueryParameters : PaginationParameters
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? BirthDate { get; set; }
}
