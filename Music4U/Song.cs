
public class Song
{
    public string Title { get; }
    public TimeOnly Time { get; }
    public DateOnly ReleaseDate { get; }
    public int SongId { get; }


    private Song(string title, TimeOnly time, DateOnly releaseDate, int songId) 
    {
        this.Title = title;
        this.Time = time;
        this.ReleaseDate = releaseDate;
        this.SongId = songId;
    }

    
    




}