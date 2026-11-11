using System.Text.Json;

namespace ToDoWebApi;

// Old: JSON file version, kept for comparison. Not used by the endpoints any more.
// Before the database, the app kept the tasks in a list and saved them to tasks.json.
// Compare these methods with TaskRepository.cs to see the difference.
public static class JsonTaskRepository
{
    // Old: JSON file version, kept for comparison. Not used by the endpoints any more.
    // All tasks. We keep them in memory and save them to a file.
    static List<TodoItem> tasks = new List<TodoItem>();

    // Old: JSON file version, kept for comparison. Not used by the endpoints any more.
    // The file where we save the tasks.
    static string tasksFile = "tasks.json";

    // Old: JSON file version, kept for comparison. Not used by the endpoints any more.
    // Finds the task with this id. Returns null if there is no such task.
    static TodoItem? FindTask(int id)
    {
        return tasks.FirstOrDefault(t => t.Id == id);
    }

    // Old: JSON file version, kept for comparison. Not used by the endpoints any more.
    // Returns the next free id: the biggest id + 1, or 1 if the list is empty.
    static int GetNextId()
    {
        if (tasks.Count == 0)
        {
            return 1;
        }

        return tasks.Max(t => t.Id) + 1;
    }

    // Old: JSON file version, kept for comparison. Not used by the endpoints any more.
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

    // Old: JSON file version, kept for comparison. Not used by the endpoints any more.
    // Writes all tasks to the file.
    static void SaveTasks()
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;

        string json = JsonSerializer.Serialize(tasks, options);
        File.WriteAllText(tasksFile, json);
    }
}
