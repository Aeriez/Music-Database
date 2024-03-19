using Renci.SshNet;
using Npgsql;

public class DatabaseManager
{
    private ForwardedPortLocal forwardedPort;
    private SshClient sshClient;
    private string databaseName = "p320_25";
    private string sshHost = "starbug.cs.rit.edu";
    private NpgsqlDataSource dataSource;

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

    public void Close()
    {
        forwardedPort?.Stop();
        sshClient?.Disconnect();
    }
}