using Renci.SshNet;
using Npgsql;

public class DatabaseManager : IDisposable
{
    private readonly string Username;
    private readonly string Password;
    private readonly ForwardedPortLocal ForwardedPort;
    private readonly SshClient SshClient;

    private const string DatabaseName = "p320_25";
    private const string SshHost = "starbug.cs.rit.edu";

    public DatabaseManager(string username, string password)
    {
        Username = username;
        Password = password;

        var connInfo = new PasswordConnectionInfo(SshHost, Username, Password);
        SshClient = new SshClient(connInfo);

        SshClient.Connect();
        //Console.WriteLine("SSH Connection Established");

        ForwardedPort = new ForwardedPortLocal("127.0.0.1", "127.0.0.1", 5432);
        SshClient.AddForwardedPort(ForwardedPort);
        ForwardedPort.Start();
        //Console.WriteLine("Port Forwarded");
    }

    public NpgsqlDataSource CreateDataSource()
    {
        var connectionString = $"Server={ForwardedPort.BoundHost};Database={DatabaseName};Port={ForwardedPort.BoundPort};User Id={Username};Password={Password};";
        return NpgsqlDataSource.Create(connectionString);
    }

    public void Dispose()
    {
        ForwardedPort?.Stop();
        SshClient?.Disconnect();
        GC.SuppressFinalize(this);
    }
}