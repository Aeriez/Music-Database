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

    /// <summary>
    /// Login a user into the database.
    /// </summary>
    /// <returns>The logged in user.</returns>
    /// <exception cref="InvalidCredentialsException">If the email or password is invalid.</exception>
    public static User Login(NpgsqlConnection conn, string email, string password)
    {
        var hash = HashPassword(password);

        const string getUserSql = @"
            SELECT user_name, first_name, last_name, creation_date
            FROM users
            WHERE user_email = @email AND password = @password
        ";

        using var command = new NpgsqlCommand(getUserSql, conn)
        {
            Parameters = { new("email", email), new("password", hash) }
        };

        string username, firstName, lastName;
        DateOnly creationDate;

        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read())
            {
                throw new InvalidCredentialsException($"Invalid email or password.");
            }

            username = reader.GetString(0);
            firstName = reader.GetString(1);
            lastName = reader.GetString(2);
            creationDate = DateOnly.FromDateTime(reader.GetDateTime(3));
        }

        var now = DateTime.Now;

        const string updateLastAccessSql = @"
            UPDATE users
            SET last_accessed = @last_access
            WHERE user_email = @email
        ";

        using var updateLastAccessCommand = new NpgsqlCommand(updateLastAccessSql, conn)
        {
            Parameters = { new("email", email), new("last_access", now) }
        };

        updateLastAccessCommand.ExecuteNonQuery();

        return new User(email, username, firstName, lastName, creationDate, now);
    }

    /// <summary>
    /// Follows a user.
    /// </summary>
    /// <returns>True if the user was successfully followed. False if the user was already followed.</returns>
    /// <exception cref="UserNotFoundException">If the user was not found.</exception>
    public bool FollowUser(NpgsqlConnection conn, string friend_email)
    {
        const string sql = @"
            INSERT INTO friends (user_email, friend_email)
            VALUES (@user_email, @friend_email)
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new("user_email", Email), new("friend_email", friend_email) }
        };

        try
        {
            command.ExecuteNonQuery();
        }

        catch (PostgresException e)
        {
            if (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return false;
            }
            else if (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new UserNotFoundException($"User with email {friend_email} not found.", e);
            }
        }

        return true;
    }

    /// <summary>
    /// Unfollows a user.
    /// </summary>
    public void UnfollowUser(NpgsqlConnection conn, string friend_email)
    {
        const string sql = @"
            DELETE FROM friends
            WHERE user_email = @user_email AND friend_email = @friend_email
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new("user_email", Email), new("friend_email", friend_email) }
        };

        command.ExecuteNonQuery();
    }

    public void PlaySong(NpgsqlConnection conn, string songId)
    {
        string sql = @"
            INSERT INTO user_listens_to_song (user_email, song_id, date_time)
            VALUES(@user_email, @song_id, @date_time)
        ";
        
        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new("user_email", Email), new("song_id", songId), new("date_time", DateTime.Now)}
        };

        command.ExecuteNonQuery();
    }

    private static byte[] HashPassword(String password)
    {
        const string salt = "Music4U";
        return SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));
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
            catch(Exception e) {}
                
        }
            
    }
}


public class DuplicateException : Exception
{
    public DuplicateException() { }
    public DuplicateException(string message) : base(message) { }
    public DuplicateException(string message, Exception inner) : base(message, inner) { }
}

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() { }
    public InvalidCredentialsException(string message) : base(message) { }
    public InvalidCredentialsException(string message, Exception inner) : base(message, inner) { }
}

public class UserNotFoundException : Exception
{
    public UserNotFoundException() { }
    public UserNotFoundException(string message) : base(message) { }
    public UserNotFoundException(string message, Exception inner) : base(message, inner) { }
}
