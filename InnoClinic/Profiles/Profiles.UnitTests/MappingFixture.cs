using Mapster;
using Profiles.BLL.Services;

namespace Profiles.UnitTests;

/// <summary>
/// Initializes Mapster global configuration once for the entire test assembly.
/// Scans Profiles.BLL for any IRegister implementations.
/// </summary>
public class MappingFixture
{
    public MappingFixture()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MedicalStaffService).Assembly);
    }
}
