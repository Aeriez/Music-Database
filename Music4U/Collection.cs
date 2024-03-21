using System.Security.Cryptography;
using System.Text;
using Npgsql;

public class Collection
{
    public int Id { get; }
    public User Useremail { get; }
    public string Name { get; }
    

    private Collection(string email, string username, string firstName, string lastName, DateOnly creationDate, DateTime lastAccess)
    {
        Email = email;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        CreationDate = creationDate;
        LastAccess = lastAccess;
    }

}