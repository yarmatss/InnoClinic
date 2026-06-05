using Appointments.Infrastructure.Connection;
using Dapper;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.GetPatientAppointments;

public class GetPatientAppointmentsHandler(ISqlConnectionFactory connectionFactory)
    : IRequestHandler<GetPatientAppointmentsQuery, Result<IEnumerable<AppointmentResponse>>>
{
    public async Task<Result<IEnumerable<AppointmentResponse>>> Handle(
        GetPatientAppointmentsQuery request, 
        CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT ""Id"", ""PatientId"", ""MedicalStaffId"", ""StartTime"", ""EndTime"", ""Status"", ""Comments""
            FROM ""Appointments""
            WHERE ""PatientId"" = @PatientId
            ORDER BY ""StartTime"" DESC";

        var appointments = await connection.QueryAsync<AppointmentResponse>(sql, new { request.PatientId });

        return Result.Success(appointments);
    }
}
