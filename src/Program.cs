namespace ToDoWebApi;

// The start of the app. Main only sets things up and connects each address to a method.
// The rules are in Services/TaskService.cs (tasks) and Services/UserService.cs (users).
// The SQL is in Repositories/TaskRepository.cs and Repositories/UserRepository.cs.
// The classes for the data (TodoItem, User and the DTOs) are in the Models folder.
// Repositories/Database.cs opens the connections and creates the tables.
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddOpenApi(o => o.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);

        WebApplication app = builder.Build();
        app.MapOpenApi();
        app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "TodoApi"));

        // Before (JSON): tasks = LoadTasks();
        // (LoadTasks and the old JSON code are now in Repositories/JsonTaskRepository.cs.)
        Database.InitializeDatabase();

        // ---------- Task endpoints (Services/TaskService.cs) ----------

        //endpoint
        app.MapGet("/todos", TaskService.GetAllTasks);
        //endpoint
        app.MapGet("/todos/open", TaskService.GetOpenTasks);
        //endpoint
        app.MapGet("/todos/search", TaskService.SearchTasks);
        //endpoint
        app.MapGet("/todos/{id}", TaskService.GetTaskById);
        //endpoint
        app.MapPost("/todos", TaskService.AddTask);
        // behind the scenes - the .NET framework - links the POST method and the
        // andress /todos to AddTask
        //endpoint
        app.MapPut("/todos/{id}", TaskService.UpdateTask);
        //endpoint
        app.MapPut("/todos/{id}/done", TaskService.MarkTaskDone);
        //endpoint
        app.MapDelete("/todos/{id}", TaskService.DeleteTask);

        // ---------- User endpoints (Services/UserService.cs) ----------

        //endpoint
        app.MapPost("/users", UserService.AddUser);
        //endpoint
        app.MapGet("/users", UserService.GetAllUsers);
        //endpoint
        app.MapGet("/users/{id}", UserService.GetUserById);
        //endpoint
        app.MapGet("/users/{id}/todos", UserService.GetTasksOfUser);

        app.Run();
    }
}
