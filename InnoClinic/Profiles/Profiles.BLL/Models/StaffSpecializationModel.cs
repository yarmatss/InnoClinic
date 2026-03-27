namespace Profiles.BLL.Models;

public class StaffSpecializationModel
{
    public Guid StaffId { get; set; }
    public Guid SpecializationId { get; set; }

    public bool IsPrimary { get; set; }
    public DateTime CertificationDate { get; set; }
}
