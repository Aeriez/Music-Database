using dotenv.net;
using Npgsql;

DotEnv.Load();

var connectionString = "Host=127.0.0.1;Port=5432;Database=p320_25";

await using var dataSource = NpgsqlDataSource.Create(connectionString);

await using var command = dataSource.CreateCommand("SELECT * FROM song");
await using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    Console.WriteLine(reader.GetString(0));
}