using Npgsql;


/// <summary>
/// Class for generating random data.
/// </summary>
public class Generator
{
    private static readonly Random random = new();

    public static readonly string[] Genres = [
        "Rock", "Pop", "Jazz", "EDM", "Rap", "Country", "Folk", "Blues",
        "R&B", "Latin", "Punk", "Hip-Hop", "Metal",
    ];

    private static readonly string[] adjectives = [
        "Eternal", "Broken", "Golden", "Frozen", "Silent", "Lonely", "Shining",
        "Mysterious", "Crimson", "Dark", "Raging", "Dancing", "Falling",
        "Pretty", "Flaming", "Red", "Blue", "Green", "Yellow", "Black",
        "Shattered", "Cursed", "Vulnerable", "Stolen", "Lost", "Forgotten",
        "Spectral", "Frightened", "Nightmare", "Doomed", "Wicked", "Cruel",
        "Foolish", "Hurt", "Angry", "Happy", "Sad", "Scared", "Calm",
        "Beautiful", "Gloomy", "Sunny", "Cloudy", "Rainy", "Snowy",
        "Windy", "Stormy", "Disguised", "Shining", "Glowing", "Dazzling",
        "Glittering", "Dazzling", "Mystic", "Ethereal", "Amazing", "Tough",
        "Kind", "Benevolent", "Malevolent", "Honest", "Clever", "Brave",
        "Blue", "Green", "Yellow", "Black", "White", "Red", "Purple",
    ];

    private static readonly string[] nouns = [
        "Heart", "Dream", "Soul", "Mind", "Body", "Time", "Space",
        "Love", "Night", "Moment", "Memory", "Day", "War", "Warrior",
        "Night", "Blood", "Fire", "Water", "Earth", "Wind", "Light",
        "Void", "Darkness", "Emotion", "Music", "Sound", "Light",
        "Shadow", "Cloud", "Tree", "Moon", "Star", "Sun", "Sky",
        "World", "Bread", "Blood", "Basket", "31 Puppies", "Toy",
        "War", "Avatar", "Crown", "Gold", "Silver", "Copper", "Brass",
        "Diamond", "Heart", "Mind", "Tax", "Goal", "Feeling", "Power",
        "Me", "You", "Us", "Friend", "Enemy", "Hate", "King", "Queen",
        "Whispers", "Journey", "Reflections", "Horizons", "Visions",
    ];

    private static readonly string[] separators = [
        "of", "in", "on", "with", "without", "under", "beyond", "above",
        "below", "through", "for", "from", "to", "and", "or", "nor",
        "but", "if", "then", "outside", "inside", "within", "behind",
        "beyond", "for", "from", "to", "except", "between", "among",
    ];

    private static readonly string[] firstNames = [
        "John", "Jane", "Mary", "Patricia", "Linda", "Barbara", "Elizabeth",
        "Jennifer", "Maria", "Susan", "Margaret", "Dorothy", "Lisa", "Nancy",
        "Karen", "Betty", "Helen", "Sandra", "Donna", "Carol", "Ruth", "Sharon",
        "Michelle", "Laura", "Sarah", "Kimberly", "Deborah", "Jessica", "Shirley",
        "Cynthia", "Angela", "Melissa", "Brenda", "Amy", "Anna", "Rebecca",
        "Virginia", "Kathleen", "Pamela", "Martha", "Debra", "Amanda", "Stephanie",
        "Carolyn", "Christine", "Marie", "Janet", "Catherine", "Frances", "Ann",
        "Joyce", "Diane", "Alice", "Julie", "Heather", "Terry", "Doris", "Gloria",
        "Evelyn", "Jean", "Cheryl", "Mildred", "Katherine", "Joan", "Ashley",
        "Judith", "Rose", "Janice", "Kelly", "Nicole", "Judy", "Christina",
        "Kathy", "Theresa", "Beverly", "Denise", "Tammy", "Irene", "Jane",
        "Lori", "Rachel", "Marilyn", "Andrea", "Kathryn", "Louise", "Sara",
        "Anne", "Janice", "Lillian", "Emily", "Natalie", "Grace", "Rose",
        "Judy", "Theresa", "Beverly", "Denise", "Tammy", "Irene", "Jane",
        "Lori", "Rachel", "Marilyn", "Andrea", "Kathryn", "Louise", "Sara",
    ];

    private static readonly string[] lastNames = [
        "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller",
        "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White",
        "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson",
        "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen",
        "Young", "Hernandez", "King", "Wright", "Lopez", "Hill", "Scott",
        "Green", "Adams", "Baker", "Gonzalez", "Nelson", "Carter", "Mitchell",
        "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans",
        "Edwards", "Collins", "Stewart", "Sanchez", "Morris", "Rogers", "Reed",
        "Cook", "Morgan", "Bell", "Murphy", "Bailey", "Rivera", "Cooper",
        "Richardson", "Cox", "Howard", "Ward", "Torres", "Peterson", "Gray",
        "Ramirez", "James", "Watson", "Brooks", "Kelly", "Sanders", "Price",
        "Bennett", "Wood", "Barnes", "Ross", "Henderson", "Coleman", "Jenkins",
        "Perry", "Powell", "Long", "Patterson", "Hughes", "Flores", "Washington",
        "Butler", "Simmons", "Foster", "Gonzales", "Bryant", "Alexander", "Russell",
        "Griffin", "Diaz", "Hayes",
    ];

    private static string[] articles = ["the", "My", "Your", "Our"];

    public static string RandomThing()
    {
        if (random.Next(3) == 0)
        {
            var adj = adjectives[random.Next(adjectives.Length)];
            return $"{adj} {RandomThing()}";
        }
        else
        {
            return nouns[random.Next(nouns.Length)];
        }
    }

    public static string RandomThingWithArticle(bool capitalize = true)
    {
        if (random.Next(3) == 0)
        {
            return RandomThing();
        }

        var article = articles[random.Next(articles.Length)];
        var thing = $"{article} {RandomThing()}";

        if (capitalize)
        {
            return char.ToUpper(thing[0]) + thing[1..];
        }
        {
            return thing;
        }
    }

    public static string RandomSong()
    {
        if (random.Next(2) == 0)
        {
            return RandomThingWithArticle();
        }

        var separator = separators[random.Next(separators.Length)];

        return $"{RandomThingWithArticle()} {separator} {RandomThingWithArticle(false)}";
    }

    public static string RandomArtist()
    {
        if (random.Next(10) == 0)
        {
            // Band name
            return RandomThingWithArticle();
        }

        var firstName = firstNames[random.Next(firstNames.Length)];
        var lastName = lastNames[random.Next(lastNames.Length)];

        return $"{firstName} {lastName}";
    }

    public static string RandomAlbum()
    {
        switch (random.Next(3))
        {
            case 0:
                return adjectives[random.Next(adjectives.Length)];
            case 1:
                return nouns[random.Next(nouns.Length)];
            default:
                return RandomThingWithArticle();
        }
    }

    public static (string, string[]) RandomAlbumWithSongs()
    {
        if (random.Next(5) == 0)
        {
            // Single
            var song = RandomSong();
            var albumName = (random.Next(3) == 0)
                ? RandomAlbum()
                : song;

            return (albumName, [song]);
        }

        var album = RandomAlbum();
        var songCount = random.Next(3) * random.Next(5) + 3;

        var songs = new string[songCount];

        for (int i = 0; i < songCount; i++)
        {
            songs[i] = RandomSong();
        }

        if (random.Next(3) == 0)
        {
            // random chance that the album name is the first or last song
            if (random.Next(2) == 0)
            {
                songs[0] = album;
            }
            else
            {
                songs[songs.Length - 1] = album;
            }
        }

        return (album, songs);
    }

    public static DateTime RandomDate(DateTime start, DateTime end)
    {
        var range = (end - start).Days;
        return start.AddDays(random.Next(range));
    }

    public static int[] PopulateArtists(NpgsqlConnection conn, int count)
    {
        var ids = new int[count];

        for (int i = 0; i < count; i++)
        {
            var name = Generator.RandomArtist();
            const string sql = @"
                INSERT INTO artist (name) VALUES (@name)
                RETURNING artist_id
            ";

            using var command = new NpgsqlCommand(sql, conn)
            {
                Parameters = { new("name", name) }
            };

#pragma warning disable CS8605 // Unboxing a possibly null value.
            ids[i] = (int)command.ExecuteScalar();
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }

        return ids;
    }

    public static void PopulateAlbum(NpgsqlConnection conn, int artistId, int[] artistIds)
    {
        var (albumName, songs) = RandomAlbumWithSongs();
        var albumDate = RandomDate(new DateTime(1980, 1, 1), DateTime.Now);

        const string albumSql = @"
            INSERT INTO album (name, release_date)
            VALUES (@name, @release_date)
            RETURNING album_id
        ";

        // Create the album
        int albumId;
        using (var command = new NpgsqlCommand(albumSql, conn))
        {
            command.Parameters.Add(new("name", albumName));
            command.Parameters.Add(new("release_date", albumDate));

#pragma warning disable CS8605 // Unboxing a possibly null value.
            albumId = (int)command.ExecuteScalar();
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }

        const string albumArtistSql = @"
            INSERT INTO artist_creates_album (artist_id, album_id)
            VALUES (@artist_id, @album_id)
        ";

        // Add the artist to the album
        using (var command = new NpgsqlCommand(albumArtistSql, conn))
        {
            command.Parameters.Add(new("artist_id", artistId));
            command.Parameters.Add(new("album_id", albumId));

            command.ExecuteNonQuery();
        }

        // Select a random set of possible genres
        var genereCount = random.Next(3) + 1;
        HashSet<int> genreIds = new();
        for (int i = 0; i < genereCount; i++)
        {
            genreIds.Add(random.Next(Genres.Length) + 1);
        }
        var genrePool = genreIds.ToArray();

        // Add the songs to the album
        for (int i = 0; i < songs.Length; i++)
        {
            var song = songs[i];

            const string songSql = @"
                INSERT INTO song (title, time, release_date)
                VALUES (@title, @time, @release_date)
                RETURNING song_id
            ";

            var durationMinutes = random.Next(2, 6);
            var durationSeconds = random.Next(60);
            var duration = new TimeSpan(0, durationMinutes, durationSeconds);

            var releaseDateOffset = random.Next(100);
            var releaseDate = albumDate - TimeSpan.FromDays(releaseDateOffset);

            // Create the song
            int songId;
            using (var command = new NpgsqlCommand(songSql, conn))
            {
                command.Parameters.Add(new("title", song));
                command.Parameters.Add(new("time", duration));
                command.Parameters.Add(new("release_date", releaseDate));

#pragma warning disable CS8605 // Unboxing a possibly null value.
                songId = (int)command.ExecuteScalar();
#pragma warning restore CS8605 // Unboxing a possibly null value.
            }

            const string albumContainsSongSql = @"
                INSERT INTO song_appears_in_album (album_id, song_id, track_num)
                VALUES (@album_id, @song_id, @track_num)
            ";

            // Add the song to the album
            using (var command = new NpgsqlCommand(albumContainsSongSql, conn))
            {
                command.Parameters.Add(new("album_id", albumId));
                command.Parameters.Add(new("song_id", songId));
                command.Parameters.Add(new("track_num", i + 1));

                command.ExecuteNonQuery();
            }

            int? featuring = null;
            // random chance of featuring another artist
            if (random.Next(10) == 0)
            {
                featuring = artistIds[random.Next(artistIds.Length)];
                if (featuring == artistId)
                {
                    featuring = null;
                }
            }

            const string songArtistSql = @"
                INSERT INTO artist_creates_song (artist_id, song_id)
                VALUES (@artist_id, @song_id)
            ";

            // Add the artist to the song
            using (var command = new NpgsqlCommand(songArtistSql, conn))
            {
                command.Parameters.Add(new("artist_id", artistId));
                command.Parameters.Add(new("song_id", songId));

                command.ExecuteNonQuery();
            }

            // Add the featuring artist to the song
            if (featuring != null)
            {
                using var command = new NpgsqlCommand(songArtistSql, conn)
                {
                    Parameters = {
                        new("artist_id", featuring),
                        new("song_id", songId),
                    }
                };

                command.ExecuteNonQuery();
            }

            // Add genres to the song
            var genreNum = random.Next(1, genrePool.Length);
            for (int j = 0; j < genreNum; j++)
            {
                const string songGenreSql = @"
                    INSERT INTO song_categorized_by_genre (song_id, genre_id)
                    VALUES (@song_id, @genre_id)
                ";

                using var command = new NpgsqlCommand(songGenreSql, conn)
                {
                    Parameters = {
                        new("song_id", songId),
                        new("genre_id", genrePool[j]),
                    }
                };

                command.ExecuteNonQuery();
            }
        }

        // Add genres to the album
        foreach (var genre in genrePool)
        {
            const string albumGenreSql = @"
                INSERT INTO album_categorized_by_genre (album_id, genre_id)
                VALUES (@album_id, @genre_id)
            ";

            using var command = new NpgsqlCommand(albumGenreSql, conn)
            {
                Parameters = {
                    new("album_id", albumId),
                    new("genre_id", genre),
                }
            };

            command.ExecuteNonQuery();
        }
    }
}
