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
using var dataSource = db.CreateDataSource();
using var conn = dataSource.OpenConnection();

var cli = new CLI(conn);

Console.WriteLine("Welcome to Music4U!");

while (cli.IsRunning)
{
    var input = Input.Get("Music4U> ");
    cli.Execute(input);
    Console.WriteLine(); // add a blank line after each command
}
