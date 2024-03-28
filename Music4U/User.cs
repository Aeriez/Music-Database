using System.Security.Cryptography;
using System.Text;
using Npgsql;

public class User
{
    public string Email { get; }
    public string Username { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime LastAccess { get; }
    public DateOnly CreationDate { get; }

    private User(string email, string username, string firstName, string lastName, DateOnly creationDate, DateTime lastAccess)
    {
        Email = email;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        CreationDate = creationDate;
        LastAccess = lastAccess;
    }

    /// <summary>
    /// Register a new user into the database.
    /// </summary>
    /// <returns>The newly created user.</returns>
    /// <exception cref="DuplicateException">If the username or email is already taken.</exception>
    public static User SignUp(NpgsqlConnection conn, string email, string username, string password, string firstName, string lastName)
    {
        var hash = HashPassword(password);
        var today = DateTime.Now;
        var todayDateOnly = DateOnly.FromDateTime(today);

        var sql = @"
            INSERT INTO users (user_email, user_name, password, first_name, last_name, creation_date, last_accessed)
            VALUES (@email, @username, @password, @first_name, @last_name, @creation_date, @last_access)
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters =
            {
                new("email", email),
                new("username", username),
                new("password", hash),
                new("first_name", firstName),
                new("last_name", lastName),
                new("creation_date", today),
                new("last_access", today)
            }
        };

        try
        {
            command.ExecuteNonQuery();
            return new User(email, username, firstName, lastName, todayDateOnly, today);
        }
        catch (PostgresException e)
        {
            if (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new DuplicateException("Email or username already exists.", e);
            }

            throw;
        }
    }

    private static byte[] HashPassword(String password)
    {
        const string salt = "Music4U";
        return SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));
    }
}

public class DuplicateException : Exception
{
    public DuplicateException() { }
    public DuplicateException(string message) : base(message) { }
    public DuplicateException(string message, Exception inner) : base(message, inner) { }
}
