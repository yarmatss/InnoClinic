using Appointments.Domain.Common;
using Appointments.Infrastructure.Connection;
using Dapper;
using Appointments.API.Authorization;
using InnoClinic.Core.Authorization;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.GetStaffAppointments;

public class GetStaffAppointmentsHandler(
    ISqlConnectionFactory connectionFactory,
    IAccessResolver accessResolver)
    : IRequestHandler<GetStaffAppointmentsQuery, Result<IEnumerable<AppointmentResponse>>>
{
    public async Task<Result<IEnumerable<AppointmentResponse>>> Handle(
        GetStaffAppointmentsQuery request, 
        CancellationToken cancellationToken)
    {
        if (!accessResolver.Check(new StaffRef(request.StaffId), Operation.Read).IsAllowed)
            return AppointmentErrors.Forbidden;

        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT ""Id"", ""PatientId"", ""MedicalStaffId"", ""StartTime"", ""EndTime"", ""Status"", ""Comments""
            FROM ""Appointments""
            WHERE ""MedicalStaffId"" = @StaffId
            ORDER BY ""StartTime"" ASC";

        var appointments = await connection.QueryAsync<AppointmentResponse>(new CommandDefinition(
            sql, 
            new { request.StaffId }, 
            cancellationToken: cancellationToken));

        return Result.Success(appointments);
    }
}
