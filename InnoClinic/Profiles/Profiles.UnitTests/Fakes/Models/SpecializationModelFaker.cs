using Bogus;
using Profiles.BLL.Models;

namespace Profiles.UnitTests.Fakes.Models;

public sealed class SpecializationModelFaker : Faker<SpecializationModel>
{
    public SpecializationModelFaker(int seed = 42)
    {
        UseSeed(seed);

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Commerce.Department());
        RuleFor(x => x.Code, f => f.Random.AlphaNumeric(5).ToUpper());
    }
}
