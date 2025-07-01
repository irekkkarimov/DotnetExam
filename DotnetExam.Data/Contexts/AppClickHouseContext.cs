using ClickHouse.Client.ADO;

namespace DotnetExam.Data.Contexts;

public class AppClickHouseContext(string connectionString)
{
    public ClickHouseConnection CreateConnection() => new ClickHouseConnection(connectionString);
}