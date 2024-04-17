using Npgsql;

public record Genre(int GenreId, string Name)
{
    public static List<(Genre, int)> GetTop5GenresForCurrentMonth(NpgsqlConnection conn)
    {
        const string sql = @"
            SELECT g.genre_id, g.name, COUNT(*) as listen_count
            FROM genre g, song_categorized_by_genre scbg, user_listens_to_song ults
            WHERE g.genre_id = scbg.genre_id
            AND ults.song_id = scbg.song_id
            AND EXTRACT(MONTH FROM ults.date_time) = EXTRACT(MONTH FROM CURRENT_DATE)
            AND EXTRACT(YEAR FROM ults.date_time) = EXTRACT(YEAR FROM CURRENT_DATE)
            GROUP BY g.genre_id, g.name
            ORDER BY COUNT(*) DESC
            LIMIT 5
        ";

        var genres = new List<(Genre, int)>();

        using var command = new NpgsqlCommand(sql, conn);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var genre = new Genre(reader.GetInt32(0), reader.GetString(1));
            int listenCount = reader.GetInt32(2);
            genres.Add((genre, listenCount));
        }

        return genres;
    }
}
