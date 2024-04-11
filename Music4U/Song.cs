
using Npgsql;

public record Song(int Id, string Title, string ArtistNames, string AlbumNames, TimeSpan Time, int ListenCount)
{
    public static List<Song> SearchSongs(NpgsqlConnection conn, SongSearchType searchType, string query)
    {
        const string nameSearchSql = @"
            WITH ListenCounts AS (
                SELECT song_id, COUNT(date_time) AS listen_count
                FROM user_listens_to_song
                GROUP BY song_id
            ),
            ArtistAlbumNames AS (
                SELECT
                    s.song_id,
                    COALESCE(string_agg(DISTINCT a.name, ', '), 'No Artist') AS artist_names,
                    COALESCE(string_agg(DISTINCT al.name || ' (id ' || al.album_id || ')', ', '), 'No Album') AS album_names
                FROM song s
                LEFT JOIN artist_creates_song acs ON s.song_id = acs.song_id
                LEFT JOIN artist a ON acs.artist_id = a.artist_id
                LEFT JOIN song_appears_in_album saia ON s.song_id = saia.song_id
                LEFT JOIN album al ON saia.album_id = al.album_id
                GROUP BY s.song_id
            )
            SELECT
                s.song_id AS id,
                s.title AS title,
                aan.artist_names as artists,
                aan.album_names as albums,
                s.time AS song_length,
                COALESCE(lc.listen_count, 0) AS listen_count
            FROM song s
            INNER JOIN ArtistAlbumNames aan ON s.song_id = aan.song_id
            LEFT JOIN ListenCounts lc ON s.song_id = lc.song_id
            WHERE
                LOWER(title) LIKE @query
            ORDER BY
                title ASC,
                artists ASC
        ";

        const string artistSearchSql = @"
            WITH MatchedArtists AS (
                SELECT artist_id
                FROM artist a
                WHERE LOWER(a.name) LIKE @query
            ),
            ListenCounts AS (
                SELECT song_id, COUNT(date_time) AS listen_count
                FROM user_listens_to_song
                GROUP BY song_id
            ),
            MatchedSongs AS (
                SELECT DISTINCT acs.song_id AS id
                FROM artist_creates_song acs
                INNER JOIN MatchedArtists ma ON acs.artist_id = ma.artist_id
            ),
            ArtistAlbumNames AS (
                SELECT
                    s.song_id,
                    COALESCE(string_agg(DISTINCT a.name, ', '), 'No Artist') AS artist_names,
                    COALESCE(string_agg(DISTINCT al.name || ' (id ' || al.album_id || ')', ', '), 'No Album') AS album_names
                FROM song s
                LEFT JOIN artist_creates_song acs ON s.song_id = acs.song_id
                LEFT JOIN artist a ON acs.artist_id = a.artist_id
                LEFT JOIN song_appears_in_album saia ON s.song_id = saia.song_id
                LEFT JOIN album al ON saia.album_id = al.album_id
                GROUP BY s.song_id
            )
            SELECT
                s.song_id AS id,
                s.title AS title,
                aan.artist_names AS artists,
                aan.album_names AS albums,
                s.time AS song_length,
                COALESCE(lc.listen_count, 0) AS listen_count
            FROM
                song s
            INNER JOIN MatchedSongs m ON s.song_id = m.id
            INNER JOIN ArtistAlbumNames aan ON s.song_id = aan.song_id
            LEFT JOIN ListenCounts lc ON s.song_id = lc.song_id
            ORDER BY
                title ASC,
                artists ASC
        ";

        const string albumSearchSql = @"
            WITH MatchedAlbums AS (
                SELECT album_id
                FROM album a
                WHERE LOWER(a.name) LIKE @query
            ),
            ListenCounts AS (
                SELECT song_id, COUNT(date_time) AS listen_count
                FROM user_listens_to_song
                GROUP BY song_id
            ),
            MatchedSongs AS (
                SELECT DISTINCT saia.song_id AS id
                FROM song_appears_in_album saia
                INNER JOIN MatchedAlbums ma ON saia.album_id = ma.album_id
            ),
            ArtistAlbumNames AS (
                SELECT
                    s.song_id,
                    COALESCE(string_agg(DISTINCT a.name, ', '), 'No Artist') AS artist_names,
                    COALESCE(string_agg(DISTINCT al.name || ' (id ' || al.album_id || ')', ', '), 'No Album') AS album_names
                FROM song s
                LEFT JOIN artist_creates_song acs ON s.song_id = acs.song_id
                LEFT JOIN artist a ON acs.artist_id = a.artist_id
                LEFT JOIN song_appears_in_album saia ON s.song_id = saia.song_id
                LEFT JOIN album al ON saia.album_id = al.album_id
                GROUP BY s.song_id
            )
            SELECT
                s.song_id AS id,
                s.title AS title,
                aan.artist_names AS artists,
                aan.album_names AS albums,
                s.time AS song_length,
                COALESCE(lc.listen_count, 0) AS listen_count
            FROM
                song s
            INNER JOIN MatchedSongs m ON s.song_id = m.id
            INNER JOIN ArtistAlbumNames aan ON s.song_id = aan.song_id
            LEFT JOIN ListenCounts lc ON s.song_id = lc.song_id
            ORDER BY
                title ASC,
                artists ASC
        ";

        const string genreSearchSql = @"
            WITH MatchedGenres AS (
                SELECT genre_id
                FROM genre g
                WHERE LOWER(g.name) LIKE @query
            ),
            ListenCounts AS (
                SELECT song_id, COUNT(date_time) AS listen_count
                FROM user_listens_to_song
                GROUP BY song_id
            ),
            MatchedSongs AS (
                SELECT DISTINCT scbg.song_id AS id
                FROM song_categorized_by_genre scbg
                INNER JOIN MatchedGenres mg ON scbg.genre_id = mg.genre_id
            ),
            ArtistAlbumNames AS (
                SELECT
                    s.song_id,
                    COALESCE(string_agg(DISTINCT a.name, ', '), 'No Artist') AS artist_names,
                    COALESCE(string_agg(DISTINCT al.name || ' (id ' || al.album_id || ')', ', '), 'No Album') AS album_names
                FROM song s
                LEFT JOIN artist_creates_song acs ON s.song_id = acs.song_id
                LEFT JOIN artist a ON acs.artist_id = a.artist_id
                LEFT JOIN song_appears_in_album saia ON s.song_id = saia.song_id
                LEFT JOIN album al ON saia.album_id = al.album_id
                GROUP BY s.song_id
            )
            SELECT
                s.song_id AS id,
                s.title AS title,
                aan.artist_names AS artists,
                aan.album_names AS albums,
                s.time AS song_length,
                COALESCE(lc.listen_count, 0) AS listen_count
            FROM
                song s
            INNER JOIN MatchedSongs m ON s.song_id = m.id
            INNER JOIN ArtistAlbumNames aan ON s.song_id = aan.song_id
            LEFT JOIN ListenCounts lc ON s.song_id = lc.song_id
            ORDER BY
                title ASC,
                artists ASC
        ";



        var sql = searchType switch
        {
            SongSearchType.Name => nameSearchSql,
            SongSearchType.Artist => artistSearchSql,
            SongSearchType.Album => albumSearchSql,
            SongSearchType.Genre => genreSearchSql,
            // Shouldn't happen, just in case
            _ => throw new Exception("Invalid search type")
        };

        using var command = new NpgsqlCommand(sql, conn)
        {
            Parameters = { new("query", $"%{query.ToLower()}%") }
        };

        using var reader = command.ExecuteReader();

        var songs = new List<Song>();

        while (reader.Read())
        {
            songs.Add(new Song(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetTimeSpan(4),
                reader.GetInt32(5)
            ));
        }

        return songs;
    }

    public static void getTop50MostPlayedSongs(NpgsqlConnection conn){
        string sql = @"
            SELECT SongId, COUNT(*) AS ListenCount
            FROM UserListensToSong
            WHERE DateTime >= DATEADD(day, -30, GETDATE())
            GROUP BY SongId
            ORDER BY ListenCount DESC
            LIMIT 50;";
           


    

        using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
        {
            command.Parameters.AddWithValue("query", $"%{sql.ToLower()}%");
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string songId = reader["SongId"].ToString();
                    int listenCount = Convert.ToInt32(reader["ListenCount"]);   
                    Console.WriteLine($"SongId: {songId}, ListenCount: {listenCount}");
                }
            }
        };


    
    }

    


 
}

