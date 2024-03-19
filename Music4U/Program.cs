using dotenv.net;

DotEnv.Load();

var username = Environment.GetEnvironmentVariable("USERNAME");
var password = Environment.GetEnvironmentVariable("PASSWORD");

if (username == null || password == null)
{
    Console.WriteLine("Please set the USERNAME and PASSWORD environment variables");
    return;
}

var db = new DatabaseManager(username, password);

await using var command = db.GetDataSource().CreateCommand("SELECT * FROM song");
await using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    Console.WriteLine(reader.GetString(0));
}

db.Close();
