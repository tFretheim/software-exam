using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace QuizApp
{
    class CategoryManager
    {


        public List<(int, string)> Categories { get; private set; }

        private DatabaseManager databaseManager;

        public CategoryManager(DatabaseManager dbManager)
        {
            Categories = new List<(int, string)>();
            databaseManager = dbManager;
            LoadCategoriesFromDatabase();
        }

        public void LoadCategoriesFromDatabase()
        {
            try
            {
                string query = "SELECT CategoryID, CategoryName FROM Categories";

                using (SQLiteDataReader reader = databaseManager.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        int categoryId = Convert.ToInt32(reader["CategoryID"]);
                        string categoryName = reader["CategoryName"].ToString();
                        Categories.Add((categoryId, categoryName));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading categories from the database: " + ex.Message);
            }
        }

        public List<(int, string)> AddCategory(string categoryName)
        {
            List<(int, string)> categories = new List<(int, string)>();

            try
            {
                Console.Write("Enter the new category name: ");
                string newCategory = Console.ReadLine();
                Console.WriteLine("Adding category to the database...");

                // Add to the database using parameters
                bool addCategoryResult = databaseManager.ExecuteNonQuery(
                    "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)",
                    new SQLiteParameter[] { new SQLiteParameter("@CategoryName", newCategory) });

                if (addCategoryResult)
                {
                    Console.WriteLine("Category added successfully.");

                    // Fetch updated categories from the database
                    categories = ViewCategories();
                }
                else
                {
                    Console.WriteLine("Failed to add category to the database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding category: " + ex.Message);
            }

            return categories;
        }


        public List<(int, string)> ViewCategories()
        {
            List<(int, string)> categories = new List<(int, string)>();
            string query = "SELECT CategoryID, CategoryName FROM Categories";

            try
            {
                using (SQLiteDataReader reader = DatabaseManager.Instance.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        int categoryId = Convert.ToInt32(reader["CategoryID"]);
                        string categoryName = reader["CategoryName"].ToString();
                        categories.Add((categoryId, categoryName));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching categories: " + ex.Message);
            }

            return categories;
        }
        public bool CategoryExists(string categoryName)
        {
            string query = "SELECT COUNT(*) FROM Categories WHERE CategoryName = @CategoryName";
            SQLiteParameter[] parameters = { new SQLiteParameter("@CategoryName", categoryName) };

            try
            {
                object result = DatabaseManager.Instance.ExecuteScalar(query, parameters);
                int categoryCount = Convert.ToInt32(result);

                // If the count is greater than 0, the category already exists
                return categoryCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking category existence: " + ex.Message);
                return false;
            }
        }
        public void AddQuestionToCategory(int categoryId, string question, string correctAnswer, string[] incorrectAnswers)
        {
            try
            {
         
                int questionId = InsertQuestion(categoryId, question, correctAnswer);

                if (incorrectAnswers.Length > 0)
                {
                    InsertIncorrectAnswers(questionId, incorrectAnswers);
                    Console.WriteLine("Incorrect answers added successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding question: " + ex.Message);
            }
        }



        public int GetCategoryIdByName(string categoryName)
        {
            string query = "SELECT CategoryID FROM Categories WHERE CategoryName = @CategoryName";
            SQLiteParameter[] parameters = { new SQLiteParameter("@CategoryName", categoryName) };

            try
            {
                object result = DatabaseManager.Instance.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting category ID by name: " + ex.Message);
                return -1; 
            }
        }

        public void ModifyCategory(int categoryId)
        {
            Console.WriteLine("Enter a question:");
            string question = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Enter the correct answer:");
            string correctAnswer = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Enter two incorrect answers, separated by a comma:");
            string[] incorrectAnswers = (Console.ReadLine() ?? string.Empty).Split(',');

            // Insert question to get ID
            int questionId = InsertQuestion(categoryId, question, correctAnswer);

            // Insert incorrect answers
            InsertIncorrectAnswers(questionId, incorrectAnswers);
        }

        private int InsertQuestion(int categoryId, string question, string correctAnswer)
        {
            try
            {
                string query = "INSERT INTO Questions (CategoryID, QuestionText, CorrectAnswer) " +
                               "VALUES (@CategoryID, @QuestionText, @CorrectAnswer); " +
                               "SELECT last_insert_rowid();"; // SQLite specific to get the last inserted ID

                SQLiteParameter[] parameters =
                {
                new SQLiteParameter("@CategoryID", categoryId),
                new SQLiteParameter("@QuestionText", question),
                new SQLiteParameter("@CorrectAnswer", correctAnswer)
            };

                object result = databaseManager.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting question: " + ex.Message);
                return -1;
            }
        }

        private void InsertIncorrectAnswers(int questionId, string[] incorrectAnswers)
        {
            try
            {
                string query = "INSERT INTO IncorrectAnswers (QuestionID, AnswerText) VALUES (@QuestionID, @AnswerText)";

                foreach (var incorrectAnswer in incorrectAnswers)
                {
                    SQLiteParameter[] parameters =
                    {
                new SQLiteParameter("@QuestionID", questionId),
                new SQLiteParameter("@AnswerText", incorrectAnswer.Trim())
            };

                    databaseManager.ExecuteNonQuery(query, parameters);
                }

                Console.WriteLine("Incorrect answers added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting incorrect answers: " + ex.Message);
            }
        }

        public void ChangeCategoryName(string oldCategoryName, string newCategoryName)
        {
            string query = "UPDATE Categories SET CategoryName = @NewCategoryName WHERE CategoryName = @OldCategoryName";
            SQLiteParameter[] parameters =
            {
        new SQLiteParameter("@NewCategoryName", newCategoryName),
        new SQLiteParameter("@OldCategoryName", oldCategoryName)
        };

            try
            {
                bool result = databaseManager.ExecuteNonQuery(query, parameters);

                if (result)
                {
                    Console.WriteLine($"Category name changed from '{oldCategoryName}' to '{newCategoryName}'.");
                    LoadCategoriesFromDatabase(); // Reload categories after the change
                }
                else
                {
                    Console.WriteLine("Failed to change category name.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error changing category name: " + ex.Message);
            }
        }
        public List<(int, string)> SortCategoriesAlphabetically()
        {
            return Categories.OrderBy(c => c.Item2).ToList();
        }

        public List<(int, string)> SortCategoriesRandomly()
        {
            Random rnd = new Random();
            return Categories.OrderBy(c => rnd.Next()).ToList();
        }

        public List<(int, string, double)> SortCategoriesBySuccessRate(int userId)
        {
            string query = @"SELECT c.CategoryID, c.CategoryName, 
                           CASE WHEN uqs.QuestionsAttempted > 0 THEN uqs.CorrectAnswers * 100.0 / uqs.QuestionsAttempted ELSE 0 END AS SuccessRate
                           FROM Categories c
                           LEFT JOIN UserQuizStats uqs ON c.CategoryID = uqs.CategoryID AND uqs.UserID = @UserID
                           GROUP BY c.CategoryID, c.CategoryName
                           ORDER BY SuccessRate DESC";

            List<(int, string, double)> sortedCategories = new List<(int, string, double)>();
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
        new SQLiteParameter("@UserID", userId)
            };

            try
            {
                using (SQLiteDataReader reader = DatabaseManager.Instance.ExecuteReader(query, parameters))
                {
                    while (reader.Read())
                    {
                        int categoryId = Convert.ToInt32(reader["CategoryID"]);
                        string categoryName = reader["CategoryName"].ToString();
                        double successRate = Convert.ToDouble(reader["SuccessRate"]);
                        sortedCategories.Add((categoryId, categoryName, successRate));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sorting categories by success rate: " + ex.Message);
            }

            return sortedCategories;
        }
        public void DeleteCategory(string categoryName)
        {
            try
            {
                string query = "DELETE FROM Categories WHERE CategoryName = @CategoryName";
                SQLiteParameter[] parameters = { new SQLiteParameter("@CategoryName", categoryName) };

                bool result = databaseManager.ExecuteNonQuery(query, parameters);

                if (result)
                {
                    Console.WriteLine($"Category '{categoryName}' deleted successfully.");
                    LoadCategoriesFromDatabase(); // Reload categories after the deletion
                }
                else
                {
                    Console.WriteLine("Failed to delete category.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting category: " + ex.Message);
            }
        }
    }
}