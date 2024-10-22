using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace QuizApp
{
    public class QuestionManager
    {
        private static readonly Random rng = new Random();

        public static List<Question> FetchQuestionsFromCategory(int categoryId, int questionCount)
        {
            List<Question> categoryQuestions = new List<Question>();
            string query = $"SELECT * FROM Questions WHERE CategoryID = @CategoryId ORDER BY RANDOM() LIMIT @QuestionCount";

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@CategoryId", categoryId),
                new SQLiteParameter("@QuestionCount", questionCount)
            };

            try
            {
                using (SQLiteDataReader reader = DatabaseManager.Instance.ExecuteReader(query, parameters))
                {
                    while (reader.Read())
                    {
                        Question question = new Question
                        {
                            QuestionID = Convert.ToInt32(reader["QuestionID"]),
                            QuestionText = reader["QuestionText"].ToString(),
                            CorrectAnswer = reader["CorrectAnswer"].ToString(),
                            WrongOption1 = reader["WrongOption1"].ToString(),
                            WrongOption2 = reader["WrongOption2"].ToString(),
                            CategoryID = categoryId
                        };
                        categoryQuestions.Add(question);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching questions: " + ex.Message);
            }

            return categoryQuestions;
        }
    }

    public class Question
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public string WrongOption1 { get; set; }
        public string WrongOption2 { get; set; }
        public int CategoryID { get; set; }
    }
}