using Mapster;

namespace Profiles.API.Mapping.Registers;

public class TimeSpanRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<TimeSpan, string>()
            .MapWith(src => src.ToString(@"hh\:mm\:ss"));

        config.ForType<TimeSpan?, string>()
            .MapWith(src => src.HasValue ? src.Value.ToString(@"hh\:mm\:ss") : string.Empty);
    }
}
