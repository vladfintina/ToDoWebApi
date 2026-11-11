namespace ToDoWebApi;

// The /users endpoints and the password check.
// This class checks the rules and chooses the answer (200, 404, 409, ...).
// It reads and saves users through UserRepository. It has no SQL.
public static class UserService
{
    // POST /users: adds a new user. The database chooses the Id.
    public static IResult AddUser(CreateUserDTO newUser)
    {
        if (string.IsNullOrWhiteSpace(newUser.Name))
        {
            return Results.BadRequest("The name is required.");
        }

        if (string.IsNullOrWhiteSpace(newUser.Password))
        {
            return Results.BadRequest("The password is required.");
        }

        string name = newUser.Name.Trim();
        if (UserRepository.GetByName(name) != null)
        {
            return Results.Conflict("A user with this name already exists.");
        }

        User user = new User(name, newUser.Password);
        user.Id = UserRepository.Add(user);

        // We never send the password back.
        return Results.Created($"/users/{user.Id}", ToUserResponse(user));
    }

    // GET /users: returns all users, without passwords.
    public static IResult GetAllUsers()
    {
        List<UserResponseDTO> users = new List<UserResponseDTO>();
        foreach (User user in UserRepository.GetAll())
        {
            users.Add(ToUserResponse(user));
        }

        return Results.Ok(users);
    }

    // GET /users/{id}: returns one user, or 404 if the id does not exist.
    public static IResult GetUserById(int id)
    {
        User? user = UserRepository.GetById(id);
        if (user == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToUserResponse(user));
    }

    // GET /users/{id}/todos: returns the tasks of one user.
    // Returns 404 if the user does not exist.
    public static IResult GetTasksOfUser(int id)
    {
        if (UserRepository.GetById(id) == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(TaskRepository.GetByUserId(id));
    }

    // Checks the password of a user. Returns false if the user does not exist
    // or the password is wrong. All endpoints that change a task use this method.
    public static bool IsCorrectPassword(int userId, string? password)
    {
        User? user = UserRepository.GetById(userId);
        if (user == null)
        {
            return false;
        }

        return user.Password == password;
    }

    // Copies a user without the password, so we can send it to the caller.
    static UserResponseDTO ToUserResponse(User user)
    {
        return new UserResponseDTO(user.Id, user.Name);
    }
}
