using Npgsql;
using System.Data;

namespace Appointments.Infrastructure.Connection;

public class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}
