namespace Profiles.BLL.Models;

public class PatientModel : PersonModel
{
    public required string InsuranceNumber { get; set; }
    public string? EmergencyContact { get; set; }

    public Guid? PrimaryDoctorId { get; set; }
}
