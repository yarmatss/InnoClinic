using Bogus;
using Profiles.BLL.Models;

namespace Profiles.UnitTests.Fakes.Models;

public sealed class StaffSpecializationModelFaker : Faker<StaffSpecializationModel>
{
    public StaffSpecializationModelFaker(int seed = 42)
    {
        UseSeed(seed);

        RuleFor(x => x.StaffId, f => f.Random.Guid());
        RuleFor(x => x.SpecializationId, f => f.Random.Guid());
        RuleFor(x => x.IsPrimary, f => f.Random.Bool());
        RuleFor(x => x.CertificationDate, f => DateOnly.FromDateTime(f.Date.Past(3)));
        RuleFor(x => x.Specialization, _ => null);
    }
}
