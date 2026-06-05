using Appointments.Infrastructure.Connection;
using Dapper;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.GetStaffAppointments;

public class GetStaffAppointmentsHandler(ISqlConnectionFactory connectionFactory)
    : IRequestHandler<GetStaffAppointmentsQuery, Result<IEnumerable<AppointmentResponse>>>
{
    public async Task<Result<IEnumerable<AppointmentResponse>>> Handle(
        GetStaffAppointmentsQuery request, 
        CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT ""Id"", ""PatientId"", ""MedicalStaffId"", ""StartTime"", ""EndTime"", ""Status"", ""Comments""
            FROM ""Appointments""
            WHERE ""MedicalStaffId"" = @StaffId
            ORDER BY ""StartTime"" ASC";

        var appointments = await connection.QueryAsync<AppointmentResponse>(sql, new { request.StaffId });

        return Result.Success(appointments);
    }
}
