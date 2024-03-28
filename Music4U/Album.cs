public class Album
{
    public string Name { get; }
    public DateOnly ReleaseDate { get; }
    public int AlbumId { get; }

    private Album(string name, DateOnly releaseDate, int albumId)
    {
        Name = name;
        ReleaseDate = releaseDate;
        AlbumId = albumId;
    }
}