using System.Text.Json;

namespace ToDoWebApi;

public class Program
{
    // All tasks. We keep them in memory and save them to a file.
    static List<TodoItem> tasks = new List<TodoItem>();

    // The file where we save the tasks.
    static string tasksFile = "tasks.json";

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddOpenApi();
        builder.Services.AddOpenApi(o => o.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);

        WebApplication app = builder.Build();
        app.MapOpenApi();
        app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "TodoApi"));

        tasks = LoadTasks();

        app.MapGet("/todos", GetAllTasks);
        app.MapGet("/todos/open", GetOpenTasks);
        app.MapGet("/todos/search", SearchTasks);
        app.MapGet("/todos/{id}", GetTaskById);
        app.MapPost("/todos", AddTask);
        app.MapPut("/todos/{id}", UpdateTask);
        app.MapPut("/todos/{id}/done", MarkTaskDone);
        app.MapDelete("/todos/{id}", DeleteTask);

        app.Run();
    }

    // GET /todos: returns all tasks.
    static IResult GetAllTasks()
    {
        return Results.Ok(tasks);
    }

    // GET /todos/{id}: returns one task, or 404 if the id does not exist.
    static IResult GetTaskById(int id)
    {
        TodoItem? task = FindTask(id);
        if (task == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(task);
    }

    // GET /todos/open: returns the tasks that are not done, sorted by due date.
    // Tasks without a due date come at the end.
    static IResult GetOpenTasks()
    {
        List<TodoItem> openTasks = tasks
            .Where(t => !t.IsDone && t.DueDate != null)
            .OrderBy(t => t.DueDate)
            .ToList();

        List<TodoItem> openTasksWithoutDate = tasks
            .Where(t => !t.IsDone && t.DueDate == null)
            .ToList();

        openTasks.AddRange(openTasksWithoutDate);
        return Results.Ok(openTasks);
    }

    // GET /todos/search?text=milk: returns the tasks whose title contains the text.
    static IResult SearchTasks(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Results.BadRequest("The search text is required.");
        }

        string searchText = text.Trim().ToLower();
        List<TodoItem> foundTasks = tasks
            .Where(t => t.Title.ToLower().Contains(searchText))
            .ToList();

        return Results.Ok(foundTasks);
    }

    // POST /todos: adds a new task. The API chooses the Id.
    static IResult AddTask(TodoItem newTask)
    {
        if (string.IsNullOrWhiteSpace(newTask.Title))
        {
            return Results.BadRequest("The title is required.");
        }

        // We copy only the title and the due date from the caller.
        TodoItem task = new TodoItem(newTask.Title.Trim());
        task.DueDate = newTask.DueDate;
        task.Id = GetNextId();

        tasks.Add(task);
        SaveTasks();

        return Results.Created($"/todos/{task.Id}", task);
    }

    // PUT /todos/{id}: changes the title and the due date of a task.
    static IResult UpdateTask(int id, TodoItem changes)
    {
        TodoItem? task = FindTask(id);
        if (task == null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(changes.Title))
        {
            return Results.BadRequest("The title is required.");
        }

        task.Title = changes.Title.Trim();
        task.DueDate = changes.DueDate;
        SaveTasks();

        return Results.Ok(task);
    }

    // PUT /todos/{id}/done: marks a task as done.
    static IResult MarkTaskDone(int id)
    {
        TodoItem? task = FindTask(id);
        if (task == null)
        {
            return Results.NotFound();
        }

        task.MarkDone();
        SaveTasks();

        return Results.Ok(task);
    }

    // DELETE /todos/{id}: deletes a task.
    static IResult DeleteTask(int id)
    {
        TodoItem? task = FindTask(id);
        if (task == null)
        {
            return Results.NotFound();
        }

        tasks.Remove(task);
        SaveTasks();

        return Results.NoContent();
    }

    // Finds the task with this id. Returns null if there is no such task.
    static TodoItem? FindTask(int id)
    {
        return tasks.FirstOrDefault(t => t.Id == id);
    }

    // Returns the next free id: the biggest id + 1, or 1 if the list is empty.
    static int GetNextId()
    {
        if (tasks.Count == 0)
        {
            return 1;
        }

        return tasks.Max(t => t.Id) + 1;
    }

    // Reads the tasks from the file. Returns an empty list if there is no file
    // or the file is broken.
    static List<TodoItem> LoadTasks()
    {
        if (!File.Exists(tasksFile))
        {
            Console.WriteLine("No tasks file yet. Starting with no tasks.");
            return new List<TodoItem>();
        }

        List<TodoItem>? loadedTasks;
        try
        {
            string json = File.ReadAllText(tasksFile);
            loadedTasks = JsonSerializer.Deserialize<List<TodoItem>>(json);
        }
        catch (JsonException)
        {
            Console.WriteLine($"Could not read {tasksFile}. Starting with no tasks.");
            return new List<TodoItem>();
        }

        if (loadedTasks == null)
        {
            return new List<TodoItem>();
        }

        Console.WriteLine($"Loaded {loadedTasks.Count} tasks:");
        foreach (TodoItem task in loadedTasks)
        {
            Console.WriteLine(task.Display());
        }

        return loadedTasks;
    }

    // Writes all tasks to the file.
    static void SaveTasks()
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;

        string json = JsonSerializer.Serialize(tasks, options);
        File.WriteAllText(tasksFile, json);
    }
}
