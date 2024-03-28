public class Artist 
{
    public string Name { get; }
    public int ArtistId { get; }

    private Artist(string name, int artistId) 
    {
        Name = name;
        ArtistId = artistId;
    }
}