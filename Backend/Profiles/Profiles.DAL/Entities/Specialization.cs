namespace Profiles.DAL.Entities;

public class Specialization : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }

    public ICollection<StaffSpecialization> StaffSpecializations { get; set; } = [];
}
