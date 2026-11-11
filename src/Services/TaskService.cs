using Microsoft.AspNetCore.Mvc;

namespace ToDoWebApi;

// The /todos endpoints.
// This class checks the rules and chooses the answer (200, 400, 401, 404, ...).
// It reads and saves tasks through TaskRepository. It has no SQL.
// The old JSON code (tasks, LoadTasks, SaveTasks, ...) is in JsonTaskRepository.cs.
public static class TaskService
{
    // GET /todos: returns all tasks.
    public static IResult GetAllTasks()
    {
        // Before (JSON): return Results.Ok(tasks);
        return Results.Ok(TaskRepository.GetAll());
    }

    // GET /todos/{id}: returns one task, or 404 if the id does not exist.
    public static IResult GetTaskById(int id)
    {
        // Before (JSON): TodoItem? task = FindTask(id);
        TodoItem? task = TaskRepository.GetById(id);
        if (task == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(task);
    }

    // GET /todos/open: returns the tasks that are not done, sorted by due date.
    // Tasks without a due date come at the end.
    public static IResult GetOpenTasks()
    {
        // We read the tasks from the database once. Then we filter them like before.
        List<TodoItem> allTasks = TaskRepository.GetAll();

        // Before (JSON): List<TodoItem> openTasks = tasks
        List<TodoItem> openTasks = allTasks
            .Where(t => !t.IsDone && t.DueDate != null)
            .OrderBy(t => t.DueDate)
            .ToList();

        // Before (JSON): List<TodoItem> openTasksWithoutDate = tasks
        List<TodoItem> openTasksWithoutDate = allTasks
            .Where(t => !t.IsDone && t.DueDate == null)
            .ToList();

        openTasks.AddRange(openTasksWithoutDate);
        return Results.Ok(openTasks);
    }

    // GET /todos/search?text=milk: returns the tasks whose title contains the text.
    public static IResult SearchTasks(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Results.BadRequest("The search text is required.");
        }

        string searchText = text.Trim().ToLower();

        // Before (JSON): List<TodoItem> foundTasks = tasks
        List<TodoItem> foundTasks = TaskRepository.GetAll()
            .Where(t => t.Title.ToLower().Contains(searchText))
            .ToList();

        return Results.Ok(foundTasks);
    }

    // POST /todos: adds a new task. The database chooses the Id.
    // The caller sends the user id and the password of that user.
    public static IResult AddTask(TodoItemDTO newTask)
    {
        if (string.IsNullOrWhiteSpace(newTask.Title))
        {
            return Results.BadRequest("The title is required.");
        }

        if (string.IsNullOrWhiteSpace(newTask.Password))
        {
            return Results.BadRequest("The password is required.");
        }

        if (UserRepository.GetById(newTask.UserId) == null)
        {
            return Results.NotFound("The user does not exist.");
        }

        if (!UserService.IsCorrectPassword(newTask.UserId, newTask.Password))
        {
            return Results.Unauthorized();
        }

        // We copy only the title, the due date and the user id from the caller.
        TodoItem task = new TodoItem(newTask.Title.Trim());
        task.DueDate = newTask.DueDate;
        task.UserId = newTask.UserId;

        // Before (JSON): task.Id = GetNextId();
        // Before (JSON): tasks.Add(task);
        // Before (JSON): SaveTasks();
        task.Id = TaskRepository.Add(task);

        // We created the task and it can be find at /todos/{task.Id}
        Results.Ok(task);
        return Results.Created($"/todos/{task.Id}", task);
    }

    // PUT /todos/{id}: changes the title and the due date of a task.
    // Only the user who created the task can do this.
    public static IResult UpdateTask(int id, UpdateTodoItemDTO changes)
    {
        // Before (JSON): TodoItem? task = FindTask(id);
        TodoItem? task = TaskRepository.GetById(id);
        if (task == null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(changes.Password))
        {
            return Results.BadRequest("The password is required.");
        }

        if (!UserService.IsCorrectPassword(task.UserId, changes.Password))
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(changes.Title))
        {
            return Results.BadRequest("The title is required.");
        }

        task.Title = changes.Title.Trim();
        task.DueDate = changes.DueDate;

        // Before (JSON): SaveTasks();
        TaskRepository.Update(task);

        return Results.Ok(task);
    }

    // PUT /todos/{id}/done: marks a task as done.
    // Only the user who created the task can do this.
    public static IResult MarkTaskDone(int id, PasswordDTO body)
    {
        // Before (JSON): TodoItem? task = FindTask(id);
        TodoItem? task = TaskRepository.GetById(id);
        if (task == null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(body.Password))
        {
            return Results.BadRequest("The password is required.");
        }

        if (!UserService.IsCorrectPassword(task.UserId, body.Password))
        {
            return Results.Unauthorized();
        }

        task.MarkDone();

        // Before (JSON): SaveTasks();
        TaskRepository.Update(task);

        return Results.Ok(task);
    }

    // DELETE /todos/{id}: deletes a task.
    // Only the user who created the task can do this.
    // [FromBody] tells .NET to read the password from the request body.
    // A DELETE request needs this, because normally it has no body.
    public static IResult DeleteTask(int id, [FromBody] PasswordDTO body)
    {
        // Before (JSON): TodoItem? task = FindTask(id);
        TodoItem? task = TaskRepository.GetById(id);
        if (task == null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(body.Password))
        {
            return Results.BadRequest("The password is required.");
        }

        if (!UserService.IsCorrectPassword(task.UserId, body.Password))
        {
            return Results.Unauthorized();
        }

        // Before (JSON): tasks.Remove(task);
        // Before (JSON): SaveTasks();
        TaskRepository.Delete(id);

        return Results.NoContent();
    }
}
