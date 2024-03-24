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
