using System;
using System.Data.SQLite;

namespace QuizApp
{
    public class UserManager
    {
        private DatabaseManager dbManager;

        public UserManager(DatabaseManager databaseManager)
        {
            dbManager = databaseManager;
        }

        public User? SignIn(string username, string password)
        {
            string query = "SELECT UserID, Username, IsAdmin FROM Users WHERE Username = @username AND Password = @password";
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
        new SQLiteParameter("@username", username),
        new SQLiteParameter("@password", password)
            };

            User? user = null;
            using (var reader = dbManager.ExecuteReader(query, parameters))
            {
                if (reader.Read())
                {
                    int userID = Convert.ToInt32(reader["UserID"]);
                    string? userUsername = reader["Username"].ToString();
                    bool isAdmin = Convert.ToBoolean(reader["IsAdmin"]);

                    if (userUsername != null)
                    {
                        user = new User(userID, userUsername, isAdmin);
                        user.SignIn();
                    }
                }
            }
            return user;
        }

        public bool CreateAccount(string username, string password)
        {
            //Check if username already exists
            if (UserExists(username))
            {
                Console.WriteLine("Username already exists. Please choose a different one.");
                return false;
            }

            //SQL query to insert a new user into the Users table
            string insertQuery = "INSERT INTO Users (Username, Password) VALUES (@username, @password)";

            //Create parameters to prevent SQL injection
            SQLiteParameter[] insertParameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@username", username),
                new SQLiteParameter("@password", password),
            };

            try
            {

                //Execute the query using DatabaseManager
                bool isSuccess = dbManager.ExecuteNonQuery(insertQuery, insertParameters);

                // Return true if the user was successfully inserted, false otherwise
                return isSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting user: {ex.Message}");
                return false;
            }
        }
        private bool UserExists(string username)
        {
            // SQL query to check if a user with the given username already exists
            string checkUserQuery = "SELECT COUNT(1) FROM Users WHERE Username = @username";

            // Create parameters to prevent SQL injection
            SQLiteParameter[] checkUserParameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@username", username)
            };

            // Execute the query using DatabaseManager
            int userExists = Convert.ToInt32(dbManager.ExecuteScalar(checkUserQuery, checkUserParameters));

            // Return true if a user with the given username already exists, false otherwise
            return userExists > 0;
        }
    }
}