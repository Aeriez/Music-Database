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

var cli = new CLI(db);

Console.WriteLine("Welcome to Music4U!");

var quit = false;
while (!quit)
{
    var input = Input.Get("Music4U> ");
    quit = await cli.Execute(input);
}
