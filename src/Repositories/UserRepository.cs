using Microsoft.Data.Sqlite;

namespace ToDoWebApi;

// Reads and writes users in the Users table of the database.
// This class only runs SQL. It does not check any rules.
// The rules (name required, name not taken, password correct, ...) are in UserService.
public static class UserRepository
{
    // All queries use parameters ($id, $name, ...) to protect against SQL injection.
    // See the comment in Database.cs.

    // Returns all users.
    public static List<User> GetAll()
    {
        List<User> foundUsers = new List<User>();

        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Password FROM Users ORDER BY Id;";

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    foundUsers.Add(ReadUserFromRow(reader));
                }
            }
        }

        return foundUsers;
    }

    // Returns the user with this id. Returns null if there is no such user.
    public static User? GetById(int id)
    {
        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Password FROM Users WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return ReadUserFromRow(reader);
                }
            }
        }

        return null;
    }

    // Returns the user with this name. Returns null if there is no such user.
    public static User? GetByName(string name)
    {
        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Password FROM Users WHERE Name = $name;";
            command.Parameters.AddWithValue("$name", name);

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return ReadUserFromRow(reader);
                }
            }
        }

        return null;
    }

    // Saves a new user. Returns the Id that the database chose.
    public static int Add(User user)
    {
        using (SqliteConnection connection = Database.OpenConnection())
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Name, Password)
                VALUES ($name, $password);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$name", user.Name);
            command.Parameters.AddWithValue("$password", user.Password);

            // ExecuteScalar returns the first value of the result: the new Id.
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    // Turns one database row into a User.
    // The columns must be in this order: Id, Name, Password.
    static User ReadUserFromRow(SqliteDataReader reader)
    {
        User user = new User(reader.GetString(1), reader.GetString(2));
        user.Id = reader.GetInt32(0);
        return user;
    }
}
