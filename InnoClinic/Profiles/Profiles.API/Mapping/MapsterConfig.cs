using Mapster;
using Profiles.API.DTOs.MedicalStaff;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;

namespace Profiles.API.Mapping;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<MedicalStaffModel, MedicalStaffResponseDto>.NewConfig()
            .Map(dest => dest.Specializations, src => src.StaffSpecializations == null
                ? Array.Empty<string>()
                : src.StaffSpecializations
                    .Where(ss => ss.Specialization != null)
                    .Select(ss => ss.Specialization!.Name));

        TypeAdapterConfig<MedicalStaffModel, MedicalStaff>.NewConfig()
            .Ignore(dest => dest.StaffSpecializations)
            .Ignore(dest => dest.IsActive);
    }
}
