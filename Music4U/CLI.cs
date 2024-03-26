using Npgsql;

public class CLI(DatabaseManager db)
{
    private readonly DatabaseManager DB = db;
    private User? User = null;

    public async Task<bool> Execute(string input)
    {
        input = input.Trim();
        var split = input.Split(' ', 2);

        if (split.Length == 0) return false;

        Command command;
        try
        {
            command = Enum.Parse<Command>(split[0], true);
        }
        catch (ArgumentException)
        {
            Console.WriteLine($"Unknown command: {split[0]}");
            return false;
        }

        var args = split.Length == 1 ? [] : split[1].Split(' ');
        return await Execute(command, args);
    }

    public async Task<bool> Execute(Command command, params string[] args)
    {
        switch (command)
        {
            case Command.Help:
                throw new NotImplementedException();
            case Command.SignUp:
                await ExecuteSignUp(args);
                break;
            case Command.Login:
                await ExecuteLogin(args);
                break;
            case Command.Logout:
                ExecuteLogout(args);
                break;
            case Command.Quit:
                if (args.Length > 0)
                {
                    Console.WriteLine("Quit command does not take any arguments.");
                    return false;
                }
                return true;
        }

        return false;
    }

    public async Task ExecuteSignUp(params string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("Sign up command does not take any arguments.");
            return;
        }

        if (User != null)
        {
            Console.WriteLine("You are already logged in.");
            return;
        }

        await using var conn = await DB.GetDataSource().OpenConnectionAsync();

        var email = Input.GetNonEmpty("Email: ");
        var username = Input.GetNonEmpty("Username: ");
        var password = Input.GetNonEmpty("Password: ");
        var firstName = Input.GetNonEmpty("First name: ");
        var lastName = Input.GetNonEmpty("Last name: ");

        User = await User.SignUp(conn, email, username, password, firstName, lastName);

        if (User != null)
        {
            Console.WriteLine($"Welcome, {User.Username}!");
        }
    }

    public async Task ExecuteLogin(params string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("Login command does not take any arguments.");
            return;
        }

        if (User != null)
        {
            Console.WriteLine("You are already logged in.");
            return;
        }

        // TODO: implement login
        var email = Input.GetNonEmpty("Email: ");
        var password = Input.GetNonEmpty("Password: ");

        throw new NotImplementedException("Login not yet implemented");
    }

    public void ExecuteLogout(params string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("Logout command does not take any arguments.");
            return;
        }

        if (User == null)
        {
            Console.WriteLine("You are not logged in.");
        }
        else
        {
            User = null;
            Console.WriteLine("You have successfully logged out.");
        }
    }
}

public class Input
{
    public static string Get(string prompt)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();

        if (input == null) Environment.Exit(0);

        return input.Trim();
    }

    public static string GetNonEmpty(string prompt)
    {
        while (true)
        {
            var input = Get(prompt);
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Input cannot be empty. Please try again.");
            }
            else
            {
                return input;
            }

        }
    }
}

public enum Command
{
    Help,
    SignUp,
    Login,
    Logout,
    Quit,
}
