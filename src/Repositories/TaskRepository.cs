using System.Globalization;
using Microsoft.Data.Sqlite;

namespace ToDoWebApi;

// Reads and writes tasks in the Todos table of the database.
// This class only runs SQL. It does not check any rules.
// The rules (title required, password correct, ...) are in TaskService.
public static class TaskRepository
{
    // All queries use parameters ($title, $id, ...) to protect against SQL injection.
    // See the comment in Database.cs.

    // Returns all tasks.
    public static List<TodoItem> GetAll()
    {
        List<TodoItem> foundTasks = new List<TodoItem>();

        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, IsDone, DueDate, UserId FROM Todos ORDER BY Id;";

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    foundTasks.Add(ReadTaskFromRow(reader));
                }
            }
        }

        return foundTasks;
    }

    // Returns the tasks of one user.
    public static List<TodoItem> GetByUserId(int userId)
    {
        List<TodoItem> foundTasks = new List<TodoItem>();

        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, IsDone, DueDate, UserId FROM Todos WHERE UserId = $userId ORDER BY Id;";
            command.Parameters.AddWithValue("$userId", userId);

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    foundTasks.Add(ReadTaskFromRow(reader));
                }
            }
        }

        return foundTasks;
    }

    // Returns the task with this id. Returns null if there is no such task.
    public static TodoItem? GetById(int id)
    {
        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, IsDone, DueDate, UserId FROM Todos WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return ReadTaskFromRow(reader);
                }
            }
        }

        return null;
    }

    // Saves a new task. Returns the Id that the database chose.
    public static int Add(TodoItem task)
    {
        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Todos (Title, IsDone, DueDate, UserId)
                VALUES ($title, $isDone, $dueDate, $userId);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$title", task.Title);
            command.Parameters.AddWithValue("$isDone", task.IsDone ? 1 : 0);
            command.Parameters.AddWithValue("$dueDate", DueDateToDbValue(task.DueDate));
            command.Parameters.AddWithValue("$userId", task.UserId);

            // ExecuteScalar returns the first value of the result: the new Id.
            int taskId = Convert.ToInt32(command.ExecuteScalar());
            
            return taskId;
        }
    }

    // Saves the title, the done state and the due date of an existing task.
    public static void Update(TodoItem task)
    {
        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Todos
                SET Title = $title, IsDone = $isDone, DueDate = $dueDate
                WHERE Id = $id;";
            command.Parameters.AddWithValue("$title", task.Title);
            command.Parameters.AddWithValue("$isDone", task.IsDone ? 1 : 0);
            command.Parameters.AddWithValue("$dueDate", DueDateToDbValue(task.DueDate));
            command.Parameters.AddWithValue("$id", task.Id);
            command.ExecuteNonQuery();
        }
    }

    // Deletes the task with this id.
    public static void Delete(int id)
    {
        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Todos WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        } // the connection ends - using handles closing the conn
    }

    // Turns one database row into a TodoItem.
    // The columns must be in this order: Id, Title, IsDone, DueDate, UserId.
    static TodoItem ReadTaskFromRow(SqliteDataReader reader)
    {
        TodoItem task = new TodoItem(reader.GetString(1));
        task.Id = reader.GetInt32(0);

        // SQLite has no true/false type. It stores 1 for done and 0 for not done.
        task.IsDone = reader.GetInt32(2) == 1; // 1 == 1 => true or 0 == 1 => false

        // The due date is saved as text, or NULL if the task has no due date.
        if (!reader.IsDBNull(3))
        {
            task.DueDate = DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        task.UserId = reader.GetInt32(4);
        return task;
    }

    // Turns a due date into the value we save in the database.
    // We save it as ISO 8601 text, for example "2026-12-09T10:29:20.1250000Z".
    // A task without a due date is saved as NULL.
    static object DueDateToDbValue(DateTime? dueDate)
    {
        if (dueDate == null)
        {
            return DBNull.Value;
        }

        return dueDate.Value.ToString("o", CultureInfo.InvariantCulture);
    }
}
