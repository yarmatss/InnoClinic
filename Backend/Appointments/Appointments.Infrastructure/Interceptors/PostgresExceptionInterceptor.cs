using Appointments.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Appointments.Infrastructure.Interceptors;

public class PostgresExceptionInterceptor : SaveChangesInterceptor
{
    private const string PostgresExclusionViolationCode = "23P01";

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        CheckAndThrow(eventData.Exception);
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        CheckAndThrow(eventData.Exception);
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private static void CheckAndThrow(Exception? exception)
    {
        if (exception is DbUpdateException { InnerException: PostgresException pgEx } && 
            pgEx.SqlState == PostgresExclusionViolationCode)
        {
            throw new ScheduleConflictException("A schedule conflict occurred.");
        }
    }
}
