using Appointments.Domain.Common;
using Appointments.Infrastructure.Connection;
using Dapper;
using InnoClinic.Core.Authorization;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.GetPatientAppointments;

public class GetPatientAppointmentsHandler(
    ISqlConnectionFactory connectionFactory,
    IUserResolver userResolver)
    : IRequestHandler<GetPatientAppointmentsQuery, Result<IEnumerable<AppointmentResponse>>>
{
    public async Task<Result<IEnumerable<AppointmentResponse>>> Handle(
        GetPatientAppointmentsQuery request, 
        CancellationToken cancellationToken)
    {
        var user = userResolver.Resolve(request.UserId);
        if (user == null)
            return AppointmentErrors.Unauthorized;

        if (user.Role == UserRole.Doctor)
            return AppointmentErrors.Forbidden;

        if (user.Role == UserRole.Patient && user.PatientId != request.PatientId)
            return AppointmentErrors.Forbidden;

        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT ""Id"", ""PatientId"", ""MedicalStaffId"", ""StartTime"", ""EndTime"", ""Status"", ""Comments""
            FROM ""Appointments""
            WHERE ""PatientId"" = @PatientId
            ORDER BY ""StartTime"" DESC";

        var appointments = await connection.QueryAsync<AppointmentResponse>(new CommandDefinition(
            sql, 
            new { request.PatientId }, 
            cancellationToken: cancellationToken));

        return Result.Success(appointments);
    }
}
