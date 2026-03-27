namespace Profiles.BLL.Models;

public abstract class PersonModel : BaseModel
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = null!;
    public string NationalId { get; set; } = null!;
    public string? ContactPhone { get; set; }
}
