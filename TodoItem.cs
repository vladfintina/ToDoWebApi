namespace ToDoWebApi;

// One task in the to-do list.
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsDone { get; set; }
    public DateTime? DueDate { get; set; }

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
