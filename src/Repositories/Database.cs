using Microsoft.Data.Sqlite;

namespace ToDoWebApi;

// Everything both services need to talk to the database.
// TaskService and UserService use this class to open connections.
public static class Database
{
    // The database where we save the users and the tasks.
    // "Data Source" is the name of the database file.
    static string connectionString = "Data Source=todos.db";

    // IMPORTANT: we always put values into SQL with parameters ($title, $id, ...).
    // We never build the SQL text by joining strings. This protects against
    // SQL injection: the database treats a parameter as data, never as SQL code.

    // Opens a connection to the database and turns on foreign keys.
    // SQLite needs "PRAGMA foreign_keys = ON;" on every new connection.
    // With it, the database refuses a task whose UserId is not a real user.
    public static SqliteConnection OpenConnection()
    {
        SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        return connection;
    }

    // Creates the tables if they do not exist yet. We call it once, at startup.
    public static void InitializeDatabase()
    {
        using (SqliteConnection connection = OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Todos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    IsDone INTEGER NOT NULL DEFAULT 0,
                    DueDate TEXT NULL,
                    UserId INTEGER NOT NULL REFERENCES Users(Id)
                );";
                
            command.ExecuteNonQuery();
        }

        Console.WriteLine("The database is ready.");
    }
}
