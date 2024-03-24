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

Console.WriteLine("Welcome to Music4U!");

AuthOption option = Input.GetAuthOption("Type 'signup' to sign up or 'login' to login: ");

User? user = null;
switch (option)
{
    case AuthOption.SignUp:
        while (user == null)
        {
            var email = Input.GetNonEmpty("Email: ");
            var username = Input.GetNonEmpty("Username: ");
            var password = Input.GetNonEmpty("Password: ");
            var firstName = Input.GetNonEmpty("First name: ");
            var lastName = Input.GetNonEmpty("Last name: ");

            user = await User.SignUp(conn, email, username, password, firstName, lastName);
        }
        break;
    case AuthOption.Login:
        while (user == null)
        {
            // TODO: implement login
            var email = Input.GetNonEmpty("Email: ");
            var password = Input.GetNonEmpty("Password: ");

            throw new NotImplementedException("Login not yet implemented");
        }
        break;
}

Console.WriteLine($"Welcome, {user.Username}!");
