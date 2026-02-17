using MySpot.Core.ValueObjects;

namespace MySpot.Core.Entities;

public class User
{
    public UserId Id { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public Username Username { get; private set; } = default!;
    public Password Password { get; private set; } = default!;
    public FullName FullName { get; private set; } = default!;
    public Role Role { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }


    public User()
    {
        
    }
    
    public User(Guid id, string email, string username, string password, string fullName, string role, DateTime createdAt)
    {
        Id = id;
        Email = email;
        Username = username;
        Password = password;
        FullName = fullName;
        Role = role;
        CreatedAt = createdAt;
    }
}
