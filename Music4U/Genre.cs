public class Genre 
{
    public string Name { get; }
    public int GenreId { get; }

    private Genre(string name, int genreId)
    {
        Name = name;
        GenreId = genreId;
    }

    public static List<Genre> GetTop5GenresForCurrentMonth(NpgsqlConnection conn)
    {
        var firstOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var firstOfNextMonth = firstOfMonth.AddMonths(1);

        const string sql = @"
            SELECT g.name, COUNT(*) AS listen_count
            FROM user_listens_to_song ul
            JOIN song_categorized_by_genre sc ON ul.song_id = sc.song_id
            JOIN genres g ON sc.genre_id = g.id
            WHERE ul.date_time >= @firstOfMonth AND ul.date_time < @firstOfNextMonth
            GROUP BY g.name
            ORDER BY listen_count DESC
            LIMIT 5
        ";

        var genres = new List<Genre>();

        using var command = new NpgsqlCommand(sql, conn);
        command.Parameters.AddWithValue("firstOfMonth", firstOfMonth);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var genre = new Genre(reader.GetString(0), -1); // Since genre ID isn't relevant for this context
            genres.Add(genre);
        }

        return genres;
    }
}