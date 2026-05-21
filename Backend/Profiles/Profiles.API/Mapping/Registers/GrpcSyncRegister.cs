using InnoClinic.Contracts.Grpc;
using Mapster;
using Profiles.DAL.Entities;

namespace Profiles.API.Mapping.Registers;

public class GrpcSyncRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MedicalStaff, SyncStaffProfileRequest>()
            .Map(dest => dest.MedicalStaffId, src => src.Id.ToString())
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.SpecializationIds, src =>
                src.StaffSpecializations != null
                ? src.StaffSpecializations.Select(ss => ss.SpecializationId.ToString())
                : Enumerable.Empty<string>())
            .Map(dest => dest.WorkingHours, src => src.WorkingHours)
            .Map(dest => dest.ScheduleOverrides, src => src.ScheduleOverrides)
            .UseDestinationValue(dest => dest.WorkingHours)
            .UseDestinationValue(dest => dest.ScheduleOverrides)
            .UseDestinationValue(dest => dest.SpecializationIds);

        config.NewConfig<WorkingHours, WorkingHoursMessage>()
            .Map(dest => dest.DayOfWeek, src => (int)src.DayOfWeek);

        config.NewConfig<ScheduleOverride, ScheduleOverrideMessage>()
            .Map(dest => dest.Date, src => src.Date.ToString("yyyy-MM-dd"));
    }
}
