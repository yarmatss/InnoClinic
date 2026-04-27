using Bogus;
using Profiles.DAL.Entities;

namespace Profiles.Tests.Common.Fakes.Entities;

public sealed class SpecializationFaker : Faker<Specialization>
{
    public SpecializationFaker(int seed = 42)
    {
        UseSeed(seed);

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Commerce.Department());
        RuleFor(x => x.Code, f => f.Random.AlphaNumeric(5).ToUpper());
        RuleFor(x => x.StaffSpecializations, _ => []);
    }
}
