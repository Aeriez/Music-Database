public class Genre 
{
    public string Name { get; }
    public int GenreId { get; }

    private Genre(string name, int genreId)
    {
        Name = name;
        GenreId = genreId;
    }
}