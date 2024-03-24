using dotenv.net;

DotEnv.Load();

var dbUsername = Environment.GetEnvironmentVariable("USERNAME");
var dbPassword = Environment.GetEnvironmentVariable("PASSWORD");

if (dbUsername == null || dbPassword == null)
{
    Console.WriteLine("Please set the USERNAME and PASSWORD environment variables");
    return;
}

using var db = new DatabaseManager(dbUsername, dbPassword);
await using var conn = await db.GetDataSource().OpenConnectionAsync();
var cli = new CLI(conn);

Console.WriteLine("Welcome to Music4U!");

var user = await cli.Authenticate();

Console.WriteLine($"Welcome, {user.Username}!");
