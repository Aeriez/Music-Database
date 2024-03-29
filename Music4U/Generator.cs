/// <summary>
/// Class for generating random data.
/// </summary>
public class Generator
{
    private static readonly Random random = new();

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
}
