namespace Profiles.BLL.Models;

public class PatientModel : PersonModel
{
    public string InsuranceNumber { get; set; } = null!;
    public string? EmergencyContact { get; set; }

    public Guid? PrimaryDoctorId { get; set; }
}
