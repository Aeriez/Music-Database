public class Collection
{
    public readonly int Id;
    public readonly string UserEmail;
    public readonly string Name;

    private Collection(int id, string userEmail, string name)
    {
        Id = id;
        UserEmail = userEmail;
        Name = name;
    }
}