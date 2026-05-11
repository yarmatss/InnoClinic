using Mapster;
using Profiles.API.DTOs.MedicalStaff;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;

namespace Profiles.API.Mapping.Registers;

public class MedicalStaffRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MedicalStaffModel, MedicalStaffResponseDto>()
            .Map(dest => dest.Specializations, src => src.StaffSpecializations == null
                ? Array.Empty<string>()
                : src.StaffSpecializations
                    .Where(ss => ss.Specialization != null)
                    .Select(ss => ss.Specialization!.Name));

        config.NewConfig<MedicalStaffModel, MedicalStaff>()
            .Ignore(dest => dest.StaffSpecializations)
            .Ignore(dest => dest.WorkingHours)
            .Ignore(dest => dest.ScheduleOverrides)
            .Ignore(dest => dest.IsActive);
    }
}
