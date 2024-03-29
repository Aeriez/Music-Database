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
            "search" => Command.Search.Parse(args),
            "follow" => Command.Follow.Parse(args),
            "unfollow" => Command.Unfollow.Parse(args),
            "playlist" => Command.Playlist.Parse(args),
            "play" => Command.Play.Parse(args),
            "quit" => new Command.Quit(),
            _ => throw new CommandParserException($"Unknown command: {args.Current}"),
        };

        if (args.MoveNext())
        {
            throw new CommandParserException($"Unexpected argument: {args.Current}");
        }

        return command;
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

            Console.WriteLine($"Welcome, {cli.CurrentUser.Username}!");
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

            var email = Input.GetNonEmpty("Email: ");
            var password = Input.GetNonEmpty("Password: ");

            try
            {
                cli.CurrentUser = User.Login(cli.Conn, email, password);

            }
            catch (InvalidCredentialsException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine($"Welcome, {cli.CurrentUser.Username}.");
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

    public record Search(SearchTarget Target) : Command
    {
        public override void Execute(CLI cli)
        {
            var searchTerm = Input.GetNonEmpty("Search: ");

            if (Target is SearchTarget.User)
            {
                Console.WriteLine($"Searching for users with email containing '{searchTerm}'...");
                List<User> users = User.SearchUsers(cli.Conn, searchTerm);

                if (users.Count == 0)
                {
                    Console.WriteLine("No users found.");
                }
                else
                {
                    Console.WriteLine($"Found {users.Count} users:");
                    foreach (var user in users)
                    {
                        Console.WriteLine($"{user.Username} ({user.Email})");
                    }
                }
            }
            else if (Target is SearchTarget.Song(var searchType))
            {
                var typeName = searchType.ToString().ToLower();
                Console.WriteLine($"Searching for songs by {typeName}...");
                List<Song> songs = Song.SearchSongs(cli.Conn, searchType, searchTerm);

                if (songs.Count == 0)
                {
                    Console.WriteLine("No songs found.");
                }
                else
                {
                    Console.WriteLine($"Found {songs.Count} songs:");
                    foreach (var song in songs)
                    {
                        Console.WriteLine($"{song.Title} by {song.ArtistNames} ({song.Time}) in {song.AlbumNames} ({song.ListenCount} listens)");
                    }
                }
            }
        }

        public static Search Parse(IEnumerator<string> args)
        {
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

                return new Command.Search(target);
            }
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

    public record Follow(string Email) : Command
    {
        public override void Execute(CLI cli)
        {
            if (cli.CurrentUser == null)
            {
                Console.WriteLine("You are not logged in.");
                return;
            }

            try
            {
                if (cli.CurrentUser.FollowUser(cli.Conn, Email))
                {
                    Console.WriteLine($"You are now following {Email}.");
                }
                else
                {
                    Console.WriteLine($"You are already following {Email}.");
                }
            }
            catch (UserNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static Follow Parse(IEnumerator<string> args)
        {
            if (!args.MoveNext())
            {
                throw new CommandParserException("Follow command requires at least one argument.");
            }

            return new Follow(args.Current);
        }
    }

    public record Unfollow(string Email) : Command
    {
        public override void Execute(CLI cli)
        {
            if (cli.CurrentUser == null)
            {
                Console.WriteLine("You are not logged in.");
                return;
            }

            cli.CurrentUser.UnfollowUser(cli.Conn, Email);
            Console.WriteLine($"You are no longer following {Email}.");
        }

        public static Unfollow Parse(IEnumerator<string> args)
        {
            if (!args.MoveNext())
            {
                throw new CommandParserException("Unfollow command requires at least one argument.");
            }

            return new Unfollow(args.Current);
        }

    }

    public record Playlist(PlaylistCommand Command) : Command
    {
        public override void Execute(CLI cli)
        {
            if (cli.CurrentUser == null)
            {
                Console.WriteLine("You are not logged in.");
                return;
            }

            Command.Execute(cli.Conn, cli.CurrentUser);
        }

        public static Playlist Parse(IEnumerator<string> args)
        {
            if (!args.MoveNext())
            {
                throw new CommandParserException("Playlist command requires at least one argument.");
            }

            PlaylistCommand command = args.Current.ToLower() switch
            {
                "list" => new PlaylistCommand.List(),
                "create" => new PlaylistCommand.Create(),
                "rename" => PlaylistCommand.Rename.Parse(args),
                "delete" => PlaylistCommand.Delete.Parse(args),
                "add" => PlaylistCommand.Add.Parse(args),
                "remove" => PlaylistCommand.Remove.Parse(args),
                _ => throw new CommandParserException($"Unknown playlist command: {args.Current}"),
            };

            return new Command.Playlist(command);
        }
    }

    public record Play(PlayTarget Target, int Id) : Command
    {
        public override void Execute(CLI cli)
        {
            if (cli.CurrentUser == null)
            {
                Console.WriteLine("You are not logged in.");
                return;
            }

            switch (Target)
            {
                case PlayTarget.Playlist:
                    // TODO
                    break;
                case PlayTarget.Song:
                    cli.CurrentUser.PlaySong(cli.Conn, Id);
                    Console.WriteLine("Song played.");
                    break;
            }
        }

        public static Play Parse(IEnumerator<string> args)
        {
            if (!args.MoveNext())
            {
                throw new CommandParserException("Play command requires 2 arguments.");
            }

            if (!Enum.TryParse(args.Current, true, out PlayTarget target))
            {
                throw new CommandParserException($"Unknown play target: {args.Current}");
            }

            if (!args.MoveNext())
            {
                throw new CommandParserException("Play command requires 2 arguments.");
            }

            if (!int.TryParse(args.Current, out int id))
            {
                throw new CommandParserException($"Unknown id: {args.Current}");
            }

            return new Play(target, id);
        }
    }

    public enum PlayTarget
    {
        Playlist,
        Song,
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

/// <summary>
/// Base class for playlist, aka collection, commands.
/// </summary>
public abstract record PlaylistCommand()
{
    public abstract void Execute(NpgsqlConnection conn, User user);

    public record List() : PlaylistCommand
    {
        public override void Execute(NpgsqlConnection conn, User user)
        {
            // TODO
        }
    }

    public record Create() : PlaylistCommand
    {
        public override void Execute(NpgsqlConnection conn, User user)
        {
            var name = Input.GetNonEmpty("Name: ");
            // TODO
        }
    }

    public record Rename(int PlaylistId) : PlaylistCommand
    {
        public override void Execute(NpgsqlConnection conn, User user)
        {
            var newName = Input.GetNonEmpty("New name: ");
            // TODO
        }

        public static Rename Parse(IEnumerator<string> args)
        {
            if (!args.MoveNext())
            {
                throw new CommandParserException("Playlist rename command requires at least one argument.");
            }

            if (int.TryParse(args.Current, out var id))
            {
                return new Rename(id);
            }
            else
            {
                throw new CommandParserException($"Invalid playlist id: {args.Current}");
            }
        }
    }

    public record Delete(int PlaylistId) : PlaylistCommand
    {
        public override void Execute(NpgsqlConnection conn, User user)
        {
            // TODO
        }

        public static Delete Parse(IEnumerator<string> args)
        {
            if (!args.MoveNext())
            {
                throw new CommandParserException("Playlist delete command requires at least one argument.");
            }

            if (int.TryParse(args.Current, out var id))
            {
                return new Delete(id);
            }
            else
            {
                throw new CommandParserException($"Invalid playlist id: {args.Current}");
            }
        }
    }

    public record Add(int PlaylistId, int SongId) : PlaylistCommand
    {
        public override void Execute(NpgsqlConnection conn, User user)
        {
            // TODO
        }

        public static Add Parse(IEnumerator<string> args)
        {
            var playlistIdRaw = args.MoveNext()
                ? args.Current
                : throw new CommandParserException("Playlist add command requires two arguments.");

            var songIdRaw = args.MoveNext()
                ? args.Current
                : throw new CommandParserException("Playlist add command requires two arguments.");

            if (!int.TryParse(playlistIdRaw, out var playlistId))
            {
                throw new CommandParserException($"Invalid playlist id: {args.Current}");
            }

            if (!int.TryParse(songIdRaw, out var songId))
            {
                throw new CommandParserException($"Invalid song id: {args.Current}");
            }

            return new Add(playlistId, songId);
        }
    }

    public record Remove(int PlaylistId, int SongId) : PlaylistCommand
    {
        public override void Execute(NpgsqlConnection conn, User user)
        {
            // TODO
        }

        public static Remove Parse(IEnumerator<string> args)
        {
            var playlistIdRaw = args.MoveNext()
                ? args.Current
                : throw new CommandParserException("Playlist add command requires two arguments.");

            var songIdRaw = args.MoveNext()
                ? args.Current
                : throw new CommandParserException("Playlist add command requires two arguments.");

            if (!int.TryParse(playlistIdRaw, out var playlistId))
            {
                throw new CommandParserException($"Invalid playlist id: {args.Current}");
            }

            if (!int.TryParse(songIdRaw, out var songId))
            {
                throw new CommandParserException($"Invalid song id: {args.Current}");
            }

            return new Remove(playlistId, songId);
        }
    }
}

public class CommandParserException(string message) : Exception(message)
{
}
