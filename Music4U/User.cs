using System.Security.Cryptography;
using System.Text;
using Npgsql;

public class User
{
    public string Email { get; }
    public string Username { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateOnly LastAccess { get; }
    public DateOnly CreationDate { get; }

    private User(string email, string username, string firstName, string lastName, DateOnly creationDate, DateOnly lastAccess)
    {
        Email = email;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        CreationDate = creationDate;
        LastAccess = lastAccess;
    }

    public static async Task<User> SignUp(NpgsqlConnection conn, string email, string username, string password, string firstName, string lastName)
    {
        var hash = HashPassword(password);
        var today = DateOnly.FromDateTime(DateTime.Now);
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

        await command.ExecuteNonQueryAsync();

        return new User(email, username, firstName, lastName, today, today);
    }

    private static byte[] HashPassword(String password)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(password));
    }
}