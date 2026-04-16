using Bogus;
using Profiles.DAL.Entities;
using Profiles.Domain.Enums;

namespace Profiles.UnitTests.Fakes.Entities;

public sealed class PatientFaker : Faker<Patient>
{
    public PatientFaker(int seed = 42)
    {
        UseSeed(seed);

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.FirstName, f => f.Name.FirstName());
        RuleFor(x => x.LastName, f => f.Name.LastName());
        RuleFor(x => x.MiddleName, f => f.Name.FirstName());
        RuleFor(x => x.NationalId, f => f.Random.AlphaNumeric(11));
        RuleFor(x => x.BirthDate, f => DateOnly.FromDateTime(
            f.Date.Past(30, DateTime.UtcNow.AddYears(-18))));
        RuleFor(x => x.Gender, f => f.PickRandom<Gender>());
        RuleFor(x => x.ContactPhone, f => f.Phone.PhoneNumber("+48#########"));
        RuleFor(x => x.InsuranceNumber, f => f.Random.AlphaNumeric(12).ToUpper());
        RuleFor(x => x.EmergencyContact, f => f.Name.FullName());
        RuleFor(x => x.PrimaryDoctorId, _ => null);
    }
}
