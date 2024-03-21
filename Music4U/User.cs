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

    public static async Task<User?> SignUp(NpgsqlConnection conn, string email, string username, string password, string firstName, string lastName)
    {
        var hash = HashPassword(password);
        var today = DateTime.Now;
        var todayDateOnly = DateOnly.FromDateTime(today);

        var sql = @"
            INSERT INTO users (user_email, user_name, password, first_name, last_name, creation_date, last_accessed)
            VALUES (@email, @username, @password, @first_name, @last_name, @creation_date, @last_access)
        ";

        await using var command = new NpgsqlCommand(sql, conn)
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
            await command.ExecuteNonQueryAsync();
            return new User(email, username, firstName, lastName, todayDateOnly, today);
        }
        catch (PostgresException e)
        {
            if (e.SqlState == "23505")
            {
                Console.WriteLine("User with the same email or username already exists");
            }
            else
            {
                Console.WriteLine($"Database Error: {e.Message}");
            }

            return null;
        }
    }

    private static byte[] HashPassword(String password)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(password));
    }
}