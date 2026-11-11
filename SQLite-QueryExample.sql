-- SQLite
SELECT * FROM Todos;

SELECT * FROM Users;

-- the one-to-many link: each todo with the name of its owner
SELECT Todos.Id, Todos.Title, Todos.IsDone, Users.Name
FROM Todos
JOIN Users ON Todos.UserId = Users.Id;
