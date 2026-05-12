using Bogus;
using Profiles.BLL.Models;
using Profiles.Domain.Enums;

namespace Profiles.UnitTests.Fakes.Models;

public sealed class MedicalStaffModelFaker : Faker<MedicalStaffModel>
{
    public MedicalStaffModelFaker(int seed = 42)
    {
        UseSeed(seed);

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.FirstName, f => f.Name.FirstName());
        RuleFor(x => x.LastName, f => f.Name.LastName());
        RuleFor(x => x.MiddleName, f => f.Name.FirstName());
        RuleFor(x => x.NationalId, f => f.Random.AlphaNumeric(11));
        RuleFor(x => x.BirthDate, f => DateOnly.FromDateTime(
            f.Date.Past(20, DateTime.UtcNow.AddYears(-20))));
        RuleFor(x => x.Gender, f => f.PickRandom<Gender>());
        RuleFor(x => x.ContactPhone, f => f.Phone.PhoneNumber("+48#########"));
        RuleFor(x => x.StaffType, f => f.PickRandom(
            StaffType.Doctor, StaffType.Nurse, StaffType.Administrator));
        RuleFor(x => x.LicenseNumber, f => f.Random.AlphaNumeric(10).ToUpper());
        RuleFor(x => x.HireDate, f => DateOnly.FromDateTime(f.Date.Past(5)));
        RuleFor(x => x.IsActive, _ => true);
        RuleFor(x => x.SupervisorId, _ => null);
        RuleFor(x => x.StaffSpecializations, _ => []);
    }
}
