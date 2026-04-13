namespace Profiles.BLL.Models;

public class PatientQueryModel : BaseQueryModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? BirthDate { get; set; }
}
