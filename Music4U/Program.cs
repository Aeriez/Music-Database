using System.Runtime.CompilerServices;
using dotenv.net;

DotEnv.Load();

var username = Environment.GetEnvironmentVariable("USERNAME");
var password = Environment.GetEnvironmentVariable("PASSWORD");

if (username == null || password == null)
{
    Console.WriteLine("Please set the USERNAME and PASSWORD environment variables");
    return;
}

using var db = new DatabaseManager(username, password);

/*
await using var command = db.GetDataSource().CreateCommand("SELECT * FROM song");
await using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    Console.WriteLine(reader.GetString(0));
}
*/


await using var conn = await db.GetDataSource().OpenConnectionAsync();
var user = await User.SignUp(conn, "a2@a.com", "test2", "test3", "test4", "test5");
