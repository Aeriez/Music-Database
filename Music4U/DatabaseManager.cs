using Renci.SshNet;
using Npgsql;

public class DatabaseManager : IDisposable
{
    private readonly ForwardedPortLocal forwardedPort;
    private readonly SshClient sshClient;
    private readonly string databaseName = "p320_25";
    private readonly string sshHost = "starbug.cs.rit.edu";
    private readonly NpgsqlDataSource dataSource;

    public DatabaseManager(string username, string password)
    {
        var connInfo = new PasswordConnectionInfo(sshHost, username, password);
        sshClient = new SshClient(connInfo);

        sshClient.Connect();
        //Console.WriteLine("SSH Connection Established");

        forwardedPort = new ForwardedPortLocal("127.0.0.1", "127.0.0.1", 5432);
        sshClient.AddForwardedPort(forwardedPort);
        forwardedPort.Start();
        //Console.WriteLine("Port Forwarded");

        var connectionString = $"Server={forwardedPort.BoundHost};Database={databaseName};Port={forwardedPort.BoundPort};User Id={username};Password={password};";
        dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public NpgsqlDataSource GetDataSource() => dataSource;

    public ValueTask<NpgsqlConnection> OpenConnectionAsync() => dataSource.OpenConnectionAsync();

    public void Dispose()
    {
        forwardedPort?.Stop();
        sshClient?.Disconnect();
        GC.SuppressFinalize(this);
    }
}