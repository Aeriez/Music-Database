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

    public static List<User> SearchUsers(NpgsqlConnection conn, string query)
    {
        const string sql = @"
            SELECT user_email, user_name, first_name, last_name, creation_date, last_accessed
            FROM users
            WHERE user_email LIKE @query
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new("query", $"%{query}%") }
        };

        using var reader = command.ExecuteReader();

        var users = new List<User>();

        while (reader.Read())
        {
            var email = reader.GetString(0);
            var username = reader.GetString(1);
            var firstName = reader.GetString(2);
            var lastName = reader.GetString(3);
            var creationDate = DateOnly.FromDateTime(reader.GetDateTime(4));
            var lastAccess = reader.GetDateTime(5);
            users.Add(new User(email, username, firstName, lastName, creationDate, lastAccess));
        }

        return users;
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

    public void PlaySong(NpgsqlConnection conn, int songId)
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

    public void PlayCollection(NpgsqlConnection conn, int collectionId)
    {
        string sql = @"
            INSERT INTO user_listens_to_song (user_email, song_id, date_time)
            SELECT @user_email, song_id, @date_time
            FROM collection_contains_song
            WHERE collection_id = @collection_id
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = {
                new("user_email", Email),
                new("collection_id", collectionId),
                new("date_time", DateTime.Now),
            }
        };

        command.ExecuteNonQuery();
    }

    private static byte[] HashPassword(String password)
    {
        const string salt = "Music4U";
        return SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));
    }

    public void CreateCollection(NpgsqlConnection conn, string name)
    {
        const string sql = @"
            INSERT INTO collection (user_email, name)
            VALUES (@user_email, @name)
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = {
                new("user_email", Email),
                new("name", name),
            }
        };

        command.ExecuteNonQuery();
    }

    public bool OwnsCollection(NpgsqlConnection conn, int collectionId)
    {
        const string sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM collection
                WHERE collection_id = @collection_id AND user_email = @user_email
            )
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = {
                new("collection_id", collectionId),
                new("user_email", Email),
            }
        };

        var result = command.ExecuteScalar();

        return result != null && (bool)result;
    }

    /// <summary>
    /// Adds a song to a collection.
    /// </summary>
    /// <returns>True if the song was successfully added. False if the song or collection was not found.</returns>
    public bool AddSongToCollection(NpgsqlConnection conn, int collectionId, int songId)
    {
        if (!OwnsCollection(conn, collectionId))
        {
            throw new InvalidCollectionException("You do not own this playlist.");
        }

        const string sql = @"
            INSERT INTO collection_contains_song (collection_id, song_id)
            VALUES (@collection_id, @song_id)
            ON CONFLICT DO NOTHING
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = {
                new("collection_id", collectionId),
                new("song_id", songId),
            }
        };

        try
        {
            command.ExecuteNonQuery();
        }
        catch (PostgresException e)
        {
            if (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return false;
            }
        }

        return true;
    }

    public void AddAlbumToCollection(NpgsqlConnection conn, int collectionId, int albumId)
    {
        if (!OwnsCollection(conn, collectionId))
        {
            throw new InvalidCollectionException("You do not own this playlist.");
        }

        const string sql = @"
            INSERT INTO collection_contains_song (collection_id, song_id)
            SELECT @collection_id, saia.song_id
            FROM song_appears_in_album saia
            WHERE saia.album_id = @album_id
            ON CONFLICT DO NOTHING
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new NpgsqlParameter("collection_id", collectionId), new NpgsqlParameter("album_id", albumId) }
        };

        command.ExecuteNonQuery();
    }

    public void DeleteSongFromCollection(NpgsqlConnection conn, int collectionId, int songId)
    {
        if (!OwnsCollection(conn, collectionId))
        {
            throw new InvalidCollectionException("You do not own this playlist.");
        }

        const string sql = @"
            DELETE FROM collection_contains_song
            WHERE collection_id = @collection_id AND song_id = @song_id
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = {
                new("collection_id", collectionId),
                new("song_id", songId),
            }
        };

        command.ExecuteNonQuery();
    }

    public void DeleteAlbumFromCollection(NpgsqlConnection conn, int collectionId, int albumId)
    {
        if (!OwnsCollection(conn, collectionId))
        {
            throw new InvalidCollectionException("You do not own this playlist.");
        }

        const string sql = @"
            DELETE FROM collection_contains_song
            WHERE collection_id = @collection_id AND song_id IN (
                SELECT song_id
                FROM song_appears_in_album
                WHERE album_id = @album_id
            )
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = {
                new("collection_id", collectionId),
                new("album_id", albumId),
            }
        };

        command.ExecuteNonQuery();
    }

    public void ChangeCollectionName(NpgsqlConnection conn, int collectionId, string newName)
    {
        if (!OwnsCollection(conn, collectionId))
        {
            throw new InvalidCollectionException("You do not own this playlist.");
        }

        const string sql = @"
            UPDATE collection
            SET name = @newName
            WHERE collection_id = @collection_id
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = {
                new("newName", newName),
                new("collection_id", collectionId)
            }
        };

        command.ExecuteNonQuery();
    }

    public void DeleteCollection(NpgsqlConnection conn, int collectionId)
    {
        if (!OwnsCollection(conn, collectionId))
        {
            throw new InvalidCollectionException("You do not own this playlist.");
        }

        const string sql = @"
            DELETE FROM collection
            WHERE collection_id = @collection_id
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new("collection_id", collectionId) }
        };

        command.ExecuteNonQuery();
    }

    public List<Collection> ListCollections(NpgsqlConnection conn)
    {
        const string sql = @"
            SELECT
                c.collection_id,
                c.name,
                COUNT(s.song_id) AS song_count,
                COALESCE(SUM(s.time), interval '0 seconds') as duration
            FROM collection c
            LEFT JOIN collection_contains_song ccs ON ccs.collection_id = c.collection_id
            LEFT JOIN song s ON s.song_id = ccs.song_id
            WHERE c.user_email = @user_email
            GROUP BY c.collection_id, c.name
            ORDER BY c.name ASC
        ";

        var collections = new List<Collection>();

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new("user_email", Email) }
        };
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            collections.Add(new Collection(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetTimeSpan(3)
            ));
        }

        return collections;
    }

    public List<(int, string, List<string>)> GetSuggestions(NpgsqlConnection conn)
    {
        const string sql = @"
            WITH UserArtistPopularity AS (
                SELECT acs.artist_id, COUNT(*) AS user_artist_count
                FROM user_listens_to_song ults
                JOIN artist_creates_song acs ON ults.song_id = acs.song_id
                WHERE ults.user_email = @user_email
                GROUP BY acs.artist_id
            ),
            SimilarUsers AS (
                SELECT DISTINCT ults2.user_email
                FROM user_listens_to_song ults
                JOIN user_listens_to_song ults2 ON ults.song_id = ults2.song_id
                WHERE ults.user_email = @user_email AND ults2.user_email != ults.user_email
            ),
            SimilarUserArtistPopularity AS (
                SELECT acs.artist_id, AVG(ults_count.user_artist_count) AS avg_artist_count
                FROM (
                    SELECT ults.user_email, acs.artist_id, COUNT(*) AS user_artist_count
                    FROM user_listens_to_song ults
                    JOIN artist_creates_song acs ON ults.song_id = acs.song_id
                    WHERE ults.user_email IN (SELECT user_email FROM SimilarUsers)
                    GROUP BY ults.user_email, acs.artist_id
                ) AS ults_count, artist_creates_song acs
                GROUP BY acs.artist_id
            ),
            CombinedArtistPopularity AS (
                SELECT COALESCE(uap.artist_id, sap.artist_id) AS artist_id,
                    COALESCE(uap.user_artist_count, 0) * 3
                        + COALESCE(sap.avg_artist_count, 0) AS combined_index
                FROM UserArtistPopularity uap
                FULL OUTER JOIN SimilarUserArtistPopularity sap ON uap.artist_id = sap.artist_id
            ),
            SongsByPopularArtists AS (
                SELECT acs.song_id,
                    COUNT(ults.song_id) AS global_listen_count,
                    SUM(CASE WHEN ults.user_email = @user_email THEN 1 ELSE 0 END) AS personal_listen_count
                FROM artist_creates_song acs
                JOIN user_listens_to_song ults ON acs.song_id = ults.song_id
                WHERE acs.artist_id IN (SELECT artist_id FROM CombinedArtistPopularity)
                GROUP BY acs.song_id
            ),
            RankedSongs AS (
                SELECT DISTINCT s.song_id,
                    s.title,
                    (spa.global_listen_count - LOG(1 + COALESCE(spa.personal_listen_count, 0)))
                        * cap.combined_index AS score
                FROM SongsByPopularArtists spa
                JOIN song s ON spa.song_id = s.song_id
                JOIN CombinedArtistPopularity cap ON cap.artist_id IN (
                    SELECT artist_id
                    FROM artist_creates_song acs
                    WHERE acs.song_id = s.song_id
                )
                ORDER BY score DESC
                LIMIT 25
            )
            SELECT rs.song_id, rs.title, a.name
            FROM RankedSongs rs, artist_creates_song acs, artist a
            WHERE rs.song_id = acs.song_id
            AND acs.artist_id = a.artist_id
            ORDER BY rs.score DESC, rs.song_id, a.name
        ";

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new("user_email", Email) }
        };

        using var reader = command.ExecuteReader();

        var suggestions = new List<(int, string, List<string>)>();
        while (reader.Read())
        {
            var songId = reader.GetInt32(0);
            var title = reader.GetString(1);
            var artistName = reader.GetString(2);

            // if this is the first time we've seen this song, add it to the list
            if (suggestions.Count == 0 || suggestions[^1].Item1 != songId)
            {
                suggestions.Add((songId, title, new List<string> { artistName }));
            }
            else
            {
                // if this is not the first time we've seen this song, add the artist to the list
                suggestions[^1].Item3.Add(artistName);
            }
        }

        return suggestions;
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

public class InvalidCollectionException : Exception
{
    public InvalidCollectionException() { }
    public InvalidCollectionException(string message) : base(message) { }
    public InvalidCollectionException(string message, Exception inner) : base(message, inner) { }
}
