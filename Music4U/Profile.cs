using Npgsql;

public record Profile(string Email, long CollectionCount, long Followers, long Following, List<(string, int)> TopArtists)
{
    public static Profile View(NpgsqlConnection conn, string email)
    {
        const string collectionSql = @"
            SELECT COUNT(*) AS collection_count
            FROM collection
            WHERE user_email = @user_email
        ";

        long collectionCount;
        using (var command = new NpgsqlCommand(collectionSql, conn))
        {
            command.Parameters.Add(new("user_email", email));
            var result = command.ExecuteScalar();
            collectionCount = result == null ? 0 : (long)result;
        }

        const string followersSql = @"
            SELECT COUNT(*) AS followers
            FROM friends
            WHERE friend_email = @user_email
        ";

        long followers;
        using (var command = new NpgsqlCommand(followersSql, conn))
        {
            command.Parameters.Add(new("user_email", email));
            var result = command.ExecuteScalar();
            followers = result == null ? 0 : (long)result;
        }

        const string followingSql = @"
            SELECT COUNT(*) AS following
            FROM friends
            WHERE user_email = @user_email
        ";

        long following;
        using (var command = new NpgsqlCommand(followingSql, conn))
        {
            command.Parameters.Add(new("user_email", email));
            var result = command.ExecuteScalar();
            following = result == null ? 0 : (long)result;
        }

        const string topArtistsSql = @"
            SELECT a.name as name, COUNT(a.artist_id) as listen_count
            FROM user_listens_to_song uls
            JOIN artist_creates_song acs ON acs.song_id = uls.song_id
            JOIN artist a ON a.artist_id = acs.artist_id
            WHERE uls.user_email = 'a@a.com'
            GROUP BY a.artist_id, a.name
            ORDER BY listen_count DESC, name ASC
            LIMIT 10
        ";

        var topArtists = new List<(string, int)>();
        using (var command = new NpgsqlCommand(topArtistsSql, conn))
        {
            command.Parameters.Add(new("user_email", email));
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                topArtists.Add((reader.GetString(0), reader.GetInt32(1)));
            }
        }

        return new Profile(email, collectionCount, followers, following, topArtists);
    }
}