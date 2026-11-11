namespace ToDoWebApi;

// One task in the to-do list.
public class TodoItem
{
    // I as a server handle
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsDone { get; set; }
    public DateTime? DueDate { get; set; }

    // The Id of the user who created this task.
    public int UserId { get; set; }

    // Makes a new task with a title. A new task is not done and has no due date.
    public TodoItem(string title)
    {
        Title = title;
    }

    // Marks the task as done.
    public void MarkDone()
    {
        IsDone = true;
    }

    // Returns the task as text, for example "[x] Buy milk".
    public string Display()
    {
        if (IsDone)
        {
            return $"[x] {Title}";
        }
        else
        {
            return $"[ ] {Title}";
        }
    }
}

public class TodoItemDTO
{   
    public string Title { get; set; }
    public bool IsDone { get; set; }
    public DateTime? DueDate { get; set; }

    // The user who creates the task, and the password of that user.
    public int UserId { get; set; }
    public string? Password { get; set; }
}


public class UpdateTodoItemDTO
{
    public string Title { get; set; }
    public DateTime? DueDate { get; set; }

    // The password of the user who created the task.
    public string? Password { get; set; }
}

// Used when the caller only has to send a password (mark done, delete).
public class PasswordDTO
{
    public string? Password { get; set; }
}