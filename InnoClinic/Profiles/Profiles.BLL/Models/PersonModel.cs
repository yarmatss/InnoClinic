namespace Profiles.BLL.Models;

public abstract class PersonModel : BaseModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime BirthDate { get; set; }
    public required string Gender { get; set; }
    public required string NationalId { get; set; }
    public string? ContactPhone { get; set; }
}
