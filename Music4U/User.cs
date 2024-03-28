using System.Security.Cryptography;
using System.Text;
using Npgsql;
using Renci.SshNet;

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
        return SHA256.HashData(Encoding.UTF8.GetBytes(password));
    }

    private static void createCollection(Collection id, Collection user_email, Collection name)
    {
        

    }

    private static void searchForSong(NpgsqlConnection conn )
    {
        //change sql statement to list 
        string sql = "SELECT * FROM Songs WHERE title LIKE @searchTerm";
        Console.WriteLine("Enter the song title to search for:");
        string searchTerm = Console.ReadLine();

        
        
        using(conn)
        {
            var command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

            try
            {
                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    Console.WriteLine("Search Results:");
                    while (reader.Read())
                        {
                            Console.WriteLine($"Title: {reader["title"]}, Duration: {reader["time"]}, Release Date: {reader["release_data"]}, ");
                        }
                    }
                }
                
            }
            

        }


        


    }
}

public class DuplicateException : Exception
{
    public DuplicateException() { }
    public DuplicateException(string message) : base(message) { }
    public DuplicateException(string message, Exception inner) : base(message, inner) { }
}
