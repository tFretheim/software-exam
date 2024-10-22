using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace QuizApp
{
    public class QuizManager
    {
        public static void StartQuizProcess(User currentUser)
        {
            DatabaseManager databaseManager = new DatabaseManager();
            CategoryManager categoryManager = new CategoryManager(databaseManager);

            var categoriesWithSuccessRate = categoryManager.SortCategoriesBySuccessRate(currentUser.UserID);
            var categories = categoriesWithSuccessRate.Select(c => (c.Item1, c.Item2)).ToList();

            DisplayCategoriesWithSuccessRate(categoriesWithSuccessRate);

            bool categoriesSelected = false;
            List<int> selectedCategoryIndices = new List<int>();

            while (!categoriesSelected)
            {
                Console.WriteLine("\nSort categories by: (A)lphabetical, (R)andom, (S)uccess Rate, or press (Enter) to continue with current sorting:");
                string sortOption = Console.ReadLine().ToUpper();

                switch (sortOption)
                {
                    case "A":
                        categories = categoryManager.SortCategoriesAlphabetically();
                        DisplayCategories(categories, categoriesWithSuccessRate.ToDictionary(c => c.Item1, c => c.Item3));
                        break;
                    case "R":
                        categories = categoryManager.SortCategoriesRandomly();
                        DisplayCategories(categories, categoriesWithSuccessRate.ToDictionary(c => c.Item1, c => c.Item3));
                        break;
                    case "S":
                        // Redisplay the categories sorted by success rate
                        DisplayCategoriesWithSuccessRate(categoriesWithSuccessRate);
                        break;
                    default:
                        Console.WriteLine("Please select categories (write the number, divide by commas):");
                        string? categoryInput = Console.ReadLine();
                        selectedCategoryIndices = ParseCategorySelection(categoryInput, categories.Count);

                        if (selectedCategoryIndices.Count > 0)
                        {
                            categoriesSelected = true;
                        }
                        break;
                }
            }

            // Map indices to actual category IDs
            var selectedCategoryIds = selectedCategoryIndices.Select(index => categories[index - 1].Item1).ToList();

            Console.WriteLine("How many questions would you like? (1-20)");
            string? numberOfQuestionsInput = Console.ReadLine();
            Utils.PauseAndClear();

            if (!int.TryParse(numberOfQuestionsInput, out int numberOfQuestions) || numberOfQuestions < 1 || numberOfQuestions > 20)
            {
                Console.WriteLine("Invalid input. Setting number of questions to 10.");
                numberOfQuestions = 10;
            }

            var categoryNames = categories.ToDictionary(cat => cat.Item1, cat => cat.Item2);
            ConductQuiz(currentUser, selectedCategoryIds, numberOfQuestions, categoryNames);
        }

        private static List<int> ParseCategorySelection(string input, int categoryCount)
        {
            var selectedIndices = new List<int>();

            if (!string.IsNullOrWhiteSpace(input))
            {
                var indices = input.Split(',').Select(id => id.Trim());
                foreach (var index in indices)
                {
                    if (int.TryParse(index, out int categoryIndex) && categoryIndex >= 1 && categoryIndex <= categoryCount)
                    {
                        selectedIndices.Add(categoryIndex);
                    }
                }
            }

            return selectedIndices;
        }

        public static void ConductQuiz(User currentUser, List<int> selectedCategoryIds, int totalQuestions, Dictionary<int, string> categoryNames)
        {
            Dictionary<int, (int attempted, int correct)> performance = new Dictionary<int, (int, int)>();
            var questions = FetchQuestions(selectedCategoryIds, totalQuestions);

            int questionNumber = 0;

            foreach (var question in questions)
            {
                questionNumber++;
                Console.WriteLine($"Question {questionNumber}/{totalQuestions}");
                Console.WriteLine($"Category: {categoryNames[question.CategoryID]}");
                Console.WriteLine(question.QuestionText);
                // Shuffle and display options
                var options = new List<string> { question.CorrectAnswer, question.WrongOption1, question.WrongOption2 };
                options = options.OrderBy(x => Guid.NewGuid()).ToList();
                for (int i = 0; i < options.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {options[i]}");
                }

                Console.Write("Your answer (1-3): ");
                string? response = Console.ReadLine();
                bool isCorrect = CheckAnswer(response, question.CorrectAnswer, options);

                Console.WriteLine(isCorrect ? "Correct!" : "Incorrect.");
                Utils.PauseAndClear(1500);

                // Update performance tracking
                if (!performance.ContainsKey(question.CategoryID))
                {
                    performance[question.CategoryID] = (0, 0);
                }
                performance[question.CategoryID] = (performance[question.CategoryID].attempted + 1,
                                                    performance[question.CategoryID].correct + (isCorrect ? 1 : 0));

            }

            // Update database for signed-in users
            if (currentUser != null && currentUser.Username != "Guest")
            {
                UpdateUserStats(currentUser.UserID, performance);
            }

            // Calculate and display the total correct answers
            int totalCorrect = performance.Sum(p => p.Value.correct);
            Console.WriteLine($"Quiz completed. You got {totalCorrect} out of {totalQuestions} questions correct.");
            Utils.WaitForEnter();
        }

        private static List<Question> FetchQuestions(List<int> categoryIds, int totalQuestions)
        {
            List<Question> questions = new List<Question>();
            int questionsPerCategory = totalQuestions / categoryIds.Count;
            int extraQuestions = totalQuestions % categoryIds.Count;

            foreach (int categoryId in categoryIds)
            {
                var categoryQuestions = QuestionManager.FetchQuestionsFromCategory(categoryId, questionsPerCategory + (extraQuestions > 0 ? 1 : 0));
                questions.AddRange(categoryQuestions);
                if (extraQuestions > 0) extraQuestions--;
            }

            // Shuffle the questions to randomize their order
            return questions.OrderBy(q => Guid.NewGuid()).ToList();
        }

        private static bool CheckAnswer(string userResponse, string correctAnswer, List<string> options)
        {
            if (int.TryParse(userResponse, out int selectedOptionIndex) && selectedOptionIndex >= 1 && selectedOptionIndex <= options.Count)
            {
                // Get the answer corresponding to the user's selected option
                string selectedAnswer = options[selectedOptionIndex - 1];
                return selectedAnswer == correctAnswer;
            }
            return false;
        }

        private static void UpdateUserStats(int userId, Dictionary<int, (int attempted, int correct)> performance)
        {
            foreach (var entry in performance)
            {
                int categoryId = entry.Key;
                int attempted = entry.Value.attempted;
                int correct = entry.Value.correct;

                // Check if there's an existing record for this user and category
                string queryCheck = "SELECT COUNT(1) FROM UserQuizStats WHERE UserID = @UserId AND CategoryID = @CategoryId";
                SQLiteParameter[] parametersCheck = new SQLiteParameter[]
                {
            new SQLiteParameter("@UserId", userId),
            new SQLiteParameter("@CategoryId", categoryId)
                };
                object result = DatabaseManager.Instance.ExecuteScalar(queryCheck, parametersCheck);

                if (Convert.ToInt32(result) > 0)
                {
                    // Update existing record
                    string queryUpdate = "UPDATE UserQuizStats SET QuestionsAttempted = QuestionsAttempted + @Attempted, CorrectAnswers = CorrectAnswers + @Correct WHERE UserID = @UserId AND CategoryID = @CategoryId";
                    SQLiteParameter[] parametersUpdate = new SQLiteParameter[]
                    {
                new SQLiteParameter("@Attempted", attempted),
                new SQLiteParameter("@Correct", correct),
                new SQLiteParameter("@UserId", userId),
                new SQLiteParameter("@CategoryId", categoryId)
                    };
                    DatabaseManager.Instance.ExecuteNonQuery(queryUpdate, parametersUpdate);
                }
                else
                {
                    // Insert new record
                    string queryInsert = "INSERT INTO UserQuizStats (UserID, CategoryID, QuestionsAttempted, CorrectAnswers) VALUES (@UserId, @CategoryId, @Attempted, @Correct)";
                    SQLiteParameter[] parametersInsert = new SQLiteParameter[]
                    {
                new SQLiteParameter("@UserId", userId),
                new SQLiteParameter("@CategoryId", categoryId),
                new SQLiteParameter("@Attempted", attempted),
                new SQLiteParameter("@Correct", correct)
                    };
                    DatabaseManager.Instance.ExecuteNonQuery(queryInsert, parametersInsert);
                }
            }
        }

        private static void DisplayCategories(List<(int, string)> categories, Dictionary<int, double> successRates)
        {
            Console.Clear();
            Console.WriteLine("Categories:");
            foreach (var category in categories)
            {
                double successRate = successRates.TryGetValue(category.Item1, out double rate) ? rate : 0.0;
                Console.WriteLine($"{category.Item1}. {category.Item2} - Success Rate: {successRate:F2}%");
            }
        }

        private static void DisplayCategoriesWithSuccessRate(List<(int, string, double)> categoriesWithSuccessRate)
        {
            Console.Clear();
            Console.WriteLine("Categories (sorted by success rate):");
            foreach (var category in categoriesWithSuccessRate)
            {
                Console.WriteLine($"{category.Item1}. {category.Item2} - Success Rate: {category.Item3:F2}%");
            }
        }

        public static void ViewCategoriesProcess(User currentUser)
        {
            DatabaseManager databaseManager = new DatabaseManager();
            CategoryManager categoryManager = new CategoryManager(databaseManager);

            var categoriesWithSuccessRate = categoryManager.SortCategoriesBySuccessRate(currentUser.UserID);
            var categories = categoriesWithSuccessRate.Select(c => (c.Item1, c.Item2)).ToList();

            DisplayCategoriesWithSuccessRate(categoriesWithSuccessRate);

            bool exitView = false;

            while (!exitView)
            {
                Console.WriteLine("\nSort categories by: (A)lphabetical, (R)andom, (S)uccess Rate, or press (E)xit to go back to main menu:");
                string sortOption = Console.ReadLine().ToUpper();

                switch (sortOption)
                {
                    case "A":
                        categories = categoryManager.SortCategoriesAlphabetically();
                        DisplayCategories(categories, categoriesWithSuccessRate.ToDictionary(c => c.Item1, c => c.Item3));
                        break;
                    case "R":
                        categories = categoryManager.SortCategoriesRandomly();
                        DisplayCategories(categories, categoriesWithSuccessRate.ToDictionary(c => c.Item1, c => c.Item3));
                        break;
                    case "S":
                        // Redisplay the categories sorted by success rate
                        DisplayCategoriesWithSuccessRate(categoriesWithSuccessRate);
                        break;
                    case "E":
                        exitView = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
    }
}