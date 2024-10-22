using System;
using System.Data;
using System.Data.SQLite;

namespace QuizApp
{
    public class DatabaseManager
    {
        private static DatabaseManager? instance = null;
        private static readonly object padlock = new object();
        private string connectionString;

        public DatabaseManager()
        {
            connectionString = @"Data Source=..\..\..\..\..\database\ProjectDatabase.db;Version=3;";
        }

        public static DatabaseManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DatabaseManager();
                    }
                    return instance;
                }
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        public object? ExecuteScalar(string query, SQLiteParameter[] parameters)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                // For logging exception details
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool ExecuteNonQuery(string query, SQLiteParameter[] parameters)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();
                        int affectedRows = command.ExecuteNonQuery();
                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public SQLiteDataReader ExecuteReader(string query, SQLiteParameter[] parameters)
        {
            SQLiteConnection conn = GetConnection();
            conn.Open();
            SQLiteCommand cmd = new SQLiteCommand(query, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}