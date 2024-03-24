using Npgsql;

public class CLI
{
    private readonly NpgsqlConnection conn;

    public CLI(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public Task<User> Authenticate()
    {
        AuthOption option = Input.GetAuthOption("Type 'signup' to sign up or 'login' to login: ");

        return option switch
        {
            AuthOption.SignUp => SignUp(),
            AuthOption.Login => Login(),
            // this should never happen, but just in case
            _ => throw new ArgumentException(option.ToString()),
        };
    }

    public async Task<User> SignUp()
    {
        while (true)
        {
            var email = Input.GetNonEmpty("Email: ");
            var username = Input.GetNonEmpty("Username: ");
            var password = Input.GetNonEmpty("Password: ");
            var firstName = Input.GetNonEmpty("First name: ");
            var lastName = Input.GetNonEmpty("Last name: ");

            var user = await User.SignUp(conn, email, username, password, firstName, lastName);

            if (user != null)
            {
                return user;
            }
        }
    }

    public async Task<User> Login()
    {
        while (true)
        {
            // TODO: implement login
            var email = Input.GetNonEmpty("Email: ");
            var password = Input.GetNonEmpty("Password: ");

            throw new NotImplementedException("Login not yet implemented");
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

    public static T Get<T>(string prompt, Func<string, Nullable<T>> converter) where T : struct
    {
        while (true)
        {
            var input = Get(prompt);
            var result = converter(input);
            if (result != null) return result.Value;
            Console.WriteLine("Invalid input. Please try again.");
        }
    }

    public static AuthOption GetAuthOption(string prompt)
    {
        return Get<AuthOption>(prompt, input => input.ToLower() switch
        {
            "signup" => AuthOption.SignUp,
            "login" => AuthOption.Login,
            _ => null
        });
    }

    public static int GetInt(string prompt)
    {
        return Get<int>(prompt, input =>
        {
            return int.TryParse(input, out var result)
                ? result
                : null;
        });
    }

}

public enum AuthOption
{
    SignUp,
    Login
}
