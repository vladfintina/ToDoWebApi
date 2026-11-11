namespace ToDoWebApi;

// One user of the to-do list. One user has many tasks.
// Plain text passwords are NOT safe in a real app. Real apps store a hash.
public class User
{
    // The database chooses the Id.
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }

    // Makes a new user with a name and a password.
    public User(string name, string password)
    {
        Name = name;
        Password = password;
    }
}

// What the caller sends to create a user.
public class CreateUserDTO
{
    public string? Name { get; set; }
    public string? Password { get; set; }
}

// What the API sends back about a user. It has no password on purpose.
public class UserResponseDTO
{
    public int Id { get; set; }
    public string Name { get; set; }

    public UserResponseDTO(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
