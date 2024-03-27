using Npgsql;

public class CLI(NpgsqlConnection conn)
{
    public readonly NpgsqlConnection Conn = conn;

    public User? CurrentUser = null;
    public bool IsLoggedIn => CurrentUser != null;
    public bool IsRunning { get; private set; } = true;

    public void StopRunning()
    {
        IsRunning = false;
    }

    public void LogoutUser()
    {
        CurrentUser = null;
    }

    public void Execute(string input)
    {
        try
        {
            ParseCommand(input)?.Execute(this);
        }
        catch (CommandParserException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static Command? ParseCommand(string input)
    {
        var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var args = ((IEnumerable<string>)split).GetEnumerator();

        return ParseCommand(args);
    }

    private static Command? ParseCommand(IEnumerator<string> args)
    {
        if (!args.MoveNext())
        {
            return null;
        }

        Command command = args.Current.ToLower() switch
        {
            "help" => new Command.Help(),
            "signup" => new Command.SignUp(),
            "login" => new Command.Login(),
            "logout" => new Command.Logout(),
            "search" => ParseSearchCommand(args),
            "quit" => new Command.Quit(),
            _ => throw new CommandParserException($"Unknown command: {args.Current}"),
        };

        if (args.MoveNext())
        {
            throw new CommandParserException($"Unexpected argument: {args.Current}");
        }

        return command;
    }

    private static Command.Search ParseSearchCommand(IEnumerator<string> args)
    {
        if (!args.MoveNext())
        {
            throw new CommandParserException("Search command requires at least one argument.");
        }

        SearchTarget target = args.Current.ToLower() switch
        {
            "user" => new SearchTarget.User(),
            "song" => new SearchTarget.Song(ParseSongSearchType(args)),
            _ => throw new CommandParserException($"Unknown search target: {args.Current}"),
        };

        // Accept search term as argument, or prompt
        var searchTerm = args.MoveNext() ? args.Current : Input.GetNonEmpty("Search: ");

        return new Command.Search(target, searchTerm);
    }

    private static SongSearchType ParseSongSearchType(IEnumerator<string> args)
    {
        if (!args.MoveNext()) return SongSearchType.Name;

        if (!Enum.TryParse(args.Current, true, out SongSearchType type))
        {
            throw new CommandParserException($"Unknown search type: {args.Current}");
        }

        return type;
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

public abstract record Command()
{
    public abstract void Execute(CLI cli);

    public record Help() : Command
    {
        public override void Execute(CLI cli)
        {
            Console.WriteLine("Help not implemented yet.");
        }
    }


    public record SignUp() : Command
    {
        public override void Execute(CLI cli)
        {
            if (cli.IsLoggedIn)
            {
                Console.WriteLine("You are already logged in.");
                return;
            }

            var email = Input.GetNonEmpty("Email: ");
            var username = Input.GetNonEmpty("Username: ");
            var password = Input.GetNonEmpty("Password: ");
            var firstName = Input.GetNonEmpty("First name: ");
            var lastName = Input.GetNonEmpty("Last name: ");

            try
            {
                cli.CurrentUser = User.SignUp(cli.Conn, email, username, password, firstName, lastName);
            }
            catch (DuplicateException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Console.Write($"Welcome, {cli.CurrentUser.Username}!");
        }
    }

    public record Login() : Command
    {
        public override void Execute(CLI cli)
        {
            if (cli.IsLoggedIn)
            {
                Console.WriteLine("You are already logged in.");
                return;
            }

            // TODO: implement login
            var email = Input.GetNonEmpty("Email: ");
            var password = Input.GetNonEmpty("Password: ");

            Console.WriteLine("Login not implemented yet.");
        }
    }

    public record Logout() : Command
    {
        public override void Execute(CLI cli)
        {
            if (!cli.IsLoggedIn)
            {
                Console.WriteLine("You are not logged in.");
            }
            else
            {
                cli.LogoutUser();
                Console.WriteLine("You have successfully logged out.");
            }

        }
    }

    public record Search(SearchTarget Target, string SearchTerm) : Command
    {
        public override void Execute(CLI cli)
        {
            if (Target is SearchTarget.User)
            {
                Console.WriteLine($"Searching for users with email containing '{SearchTerm}'...");
                Console.WriteLine("Not implemented yet.");
            }
            else if (Target is SearchTarget.Song(var searchType))
            {
                Console.WriteLine("Searching for songs...");
                Console.WriteLine("Not implemented yet.");
            }
        }
    }

    public record Quit() : Command
    {
        public override void Execute(CLI cli)
        {
            cli.StopRunning();
            Console.WriteLine("Goodbye!");
        }
    }
}

public abstract record SearchTarget()
{
    public record User() : SearchTarget;
    public record Song(SongSearchType SearchType) : SearchTarget;
}

public enum SongSearchType
{
    Name,
    Artist,
    Album,
    Genre,
}

public class CommandParserException(string message) : Exception(message)
{
}
