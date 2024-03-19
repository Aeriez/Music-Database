using dotenv.net;
using Npgsql;
using Renci.SshNet;

DotEnv.Load();

var username = Environment.GetEnvironmentVariable("USERNAME");
var password = Environment.GetEnvironmentVariable("PASSWORD");
const string database = "p320_25";
const string sshHost = "starbug.cs.rit.edu";

var connInfo = new PasswordConnectionInfo(sshHost, username, password);

using (var sshClient = new SshClient(connInfo))
{
    sshClient.Connect();
    Console.WriteLine("Connection Established");

    var port = new ForwardedPortLocal("127.0.0.1", "127.0.0.1", 5432);
    sshClient.AddForwardedPort(port);
    port.Start();
    Console.WriteLine("Port Forwarded");

    var connectionString = $"Server={port.BoundHost};Database={database};Port={port.BoundPort};User Id={username};Password={password};";
    await using var dataSource = NpgsqlDataSource.Create(connectionString);

    await using var command = dataSource.CreateCommand("SELECT * FROM song");
    await using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        Console.WriteLine(reader.GetString(0));
    }
}

