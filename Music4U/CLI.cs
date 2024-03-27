using Npgsql;

public class CLI(NpgsqlConnection conn)
{
    private readonly NpgsqlConnection Conn = conn;
    private User? CurrentUser = null;

    public bool Execute(string input)
    {
        input = input.Trim();

        // We need to cast this for some reason
        var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var args = ((IEnumerable<string>)split).GetEnumerator();

        if (!args.MoveNext()) return false;

        if (!Enum.TryParse(args.Current, true, out Command command))
        {
            Console.WriteLine($"Unknown command: {args.Current}");
            return false;
        }

        return Execute(command, args);
    }

    public bool Execute(Command command, IEnumerator<string> args)
    {
        switch (command)
        {
            case Command.Help:
                throw new NotImplementedException();
            case Command.SignUp:
                ExecuteSignUp();
                break;
            case Command.Login:
                ExecuteLogin();
                break;
            case Command.Logout:
                ExecuteLogout();
                break;
            case Command.Quit:
                return true;
        }

        // Ignore all uncaptured arguments
        while (args.MoveNext())
        {
            Console.WriteLine($"Ignoring argument: {args.Current}");
        }

        return false;
    }

    public void ExecuteSignUp()
    {
        if (CurrentUser != null)
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
            CurrentUser = User.SignUp(Conn, email, username, password, firstName, lastName);
        }
        catch (DuplicateException e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        Console.Write($"Welcome, {CurrentUser.Username}!");
    }

    public void ExecuteLogin()
    {
        if (CurrentUser != null)
        {
            Console.WriteLine("You are already logged in.");
            return;
        }

        // TODO: implement login
        var email = Input.GetNonEmpty("Email: ");
        var password = Input.GetNonEmpty("Password: ");

        Console.WriteLine("Login not implemented yet.");
    }

    public void ExecuteLogout(params string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("Logout command does not take any arguments.");
            return;
        }

        if (CurrentUser == null)
        {
            Console.WriteLine("You are not logged in.");
        }
        else
        {
            CurrentUser = null;
            Console.WriteLine("You have successfully logged out.");
        }
    }

    public void ExecuteSearch(IEnumerator<string> args)
    {
        if (!args.MoveNext())
        {
            Console.WriteLine("Search command requires at least one argument.");
            return;
        }

        if (!Enum.TryParse(args.Current, true, out SearchTarget target))
        {
            Console.WriteLine($"Unknown search target: {args.Current}");
            return;
        }

        switch (target)
        {
            case SearchTarget.User:
                // Accept search term as argument, or prompt
                var searchTerm = args.MoveNext() ? args.Current : Input.GetNonEmpty("Search: ");
                Console.WriteLine($"Searching for users with email containing '{searchTerm}'...");
                Console.WriteLine("Not implemented yet.");
                break;
            case SearchTarget.Song:
                ExecuteSearchSong(args);
                break;
        }
    }

    public void ExecuteSearchSong(IEnumerator<string> args)
    {
        SongSearchType searchType = SongSearchType.Name;
        // If the user provides a search type, use it
        if (args.MoveNext() && !Enum.TryParse(args.Current, true, out searchType))
        {
            var lowercaseTypes = Enum.GetNames<SongSearchType>().Select(x => x.ToLower());
            Console.WriteLine($"Unknown search type: {args.Current}");
            Console.WriteLine($"Expected: {string.Join(", ", lowercaseTypes)}");
            return;
        }

        // Accept search term as argument, or prompt
        var searchTerm = args.MoveNext() ? args.Current : Input.GetNonEmpty("Search: ");

        Console.WriteLine("Searching for songs...");
        Console.WriteLine("Not implemented yet.");
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
    Search,
    Quit,
}

public enum SearchTarget
{
    User,
    Song,
}

public enum SongSearchType
{
    Name,
    Artist,
    Album,
    Genre,
}
