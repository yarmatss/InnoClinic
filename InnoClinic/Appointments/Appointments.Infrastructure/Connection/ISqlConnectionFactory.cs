using System.Data;

namespace Appointments.Infrastructure.Connection;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
