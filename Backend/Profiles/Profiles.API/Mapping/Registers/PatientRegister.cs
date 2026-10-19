using Mapster;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;

namespace Profiles.API.Mapping.Registers;

public class PatientRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PatientModel, Patient>()
            .Ignore(dest => dest.UserId);
    }
}
