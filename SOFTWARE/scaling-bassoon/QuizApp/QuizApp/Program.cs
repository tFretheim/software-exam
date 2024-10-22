using System;
using System.Data.SQLite;
using System.Threading;

namespace QuizApp
{
    class Program
    {

        private static DatabaseManager databaseManager = DatabaseManager.Instance;
        private static CategoryManager categoryManager = new CategoryManager(databaseManager);


        private static User? currentUser;

        static void Main(string[] args)
        {


            while (true)
            {
                if (currentUser != null && (currentUser.IsLoggedIn || currentUser.Username == "Guest"))
                {
                    DisplayMainMenu();
                }
                else
                {
                    DisplayInitialMenu();
                }
            }
        }

        static void DisplayInitialMenu()
        {
            Console.WriteLine("====================================");
            Console.WriteLine("Welcome to the Quiz App!");
            Console.WriteLine("====================================");
            Console.WriteLine("1. Sign In");
            Console.WriteLine("2. Create Account");
            Console.WriteLine("3. Play as Guest");
            Console.WriteLine("4. Exit");
            Console.Write("Select an option: ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    SignIn();
                    break;
                case 2:
                    CreateAccount();
                    break;
                case 3:
                    PlayAsGuest();
                    break;
                case 4:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }

        static void SignIn()
        {
            Utils.ClearConsole();
            Console.WriteLine("------------ Sign In ------------");
            Console.Write("Enter username: ");
            string username = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter password: ");
            string password = Console.ReadLine() ?? string.Empty;

            UserManager userManager = new UserManager(DatabaseManager.Instance);
            User? signedInUser = userManager.SignIn(username, password);

            if (signedInUser != null)
            {
                currentUser = signedInUser;
                Console.WriteLine("Sign-in successful!");
            }
            else
            {
                Console.WriteLine("Invalid username or password.");
            }
            Utils.PauseAndClear();
        }

        static void CreateAccount()
        {
            Utils.ClearConsole();
            Console.WriteLine("--------- Create Account ---------");
            Console.Write("Enter preferred username: ");
            string username = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter preferred password: ");
            string password = Console.ReadLine() ?? string.Empty;

            UserManager userManager = new UserManager(DatabaseManager.Instance);

            bool isAccountCreated = userManager.CreateAccount(username, password);

            if (isAccountCreated)
            {
                Console.WriteLine("Account created successfully, you can now sign in.");
            }
            Utils.WaitForEnter();
        }

        static void PlayAsGuest()
        {
            currentUser = new User(0, "Guest", false);
            Console.WriteLine("You are now playing as a guest.");
            Utils.PauseAndClear();
        }

        static void DisplayMainMenu()
        {
            Console.WriteLine(GetCurrentUserStatus());
            Console.WriteLine("============== Main Menu ==============");
            Console.WriteLine("1. Play");
            Console.WriteLine("2. View Categories");

            if (currentUser != null && currentUser.IsAdmin)
            {
                Console.WriteLine("3. Modify Categories"); // Option only available to admins
                Console.WriteLine("4. Log out");
                Console.WriteLine("5. Exit");
            }
            else
            {
                Console.WriteLine("3. Log out");
                Console.WriteLine("4. Exit");
            }

            Console.Write("Select an option: ");
            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Proceeding to quiz..");
                    QuizManager.StartQuizProcess(currentUser!);
                    break;
                case 2:
                    QuizManager.ViewCategoriesProcess(currentUser!);
                    break;
                case 3:
                    if (currentUser != null && currentUser.IsAdmin)
                    {
                        ModifyCategories(); // New method for category modification
                    }
                    else
                    {
                        LogOut();
                    }
                    break;
                case 4:
                    if (currentUser != null && currentUser.IsAdmin)
                    {
                        LogOut();
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                    break;
                case 5:
                    if (currentUser != null && currentUser.IsAdmin)
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("Invalid option. Please try again."); // In case a non-admin user enters 5
                    }
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }

        static void LogOut()
        {
            if (currentUser != null)
            {
                currentUser = null;
                Console.WriteLine("Logged out successfully.");
                Utils.PauseAndClear();
            }
        }

        static void ModifyCategories()
        {
            bool continueEditing = true;
            while (continueEditing)
            {
                Console.WriteLine("========== Category Management ==========");
                Console.WriteLine("1. Add Category");
                Console.WriteLine("2. Modify Category");
                Console.WriteLine("3. Exit Category Management");
                Console.Write("Select an option: ");

                int choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        AddCategory(categoryManager);
                        break;
                    case 2:
                        ModifyExistingCategory();
                        break;
                    case 3:
                        continueEditing = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static void AddCategory(CategoryManager categoryManager)
        {
            Console.WriteLine("Enter the name of the new category:");
            string categoryName = Console.ReadLine() ?? string.Empty;

            // Checks if the category already exists.
            if (categoryManager.CategoryExists(categoryName))
            {
                Console.WriteLine("Category already exists. Please choose a different name.");
                return;
            }

            // Create a new category
            categoryManager.AddCategory(categoryName);

            // Inform the user that the category has been added successfully
            Console.WriteLine($"Category '{categoryName}' added successfully!");

           

            // Inform the user that questions have been added
            Console.WriteLine($"Questions added to the category '{categoryName}'.");
        }


        static void ModifyExistingCategory()
        {
 
            if (currentUser != null && currentUser.IsAdmin)
            {
                Console.WriteLine("========== Modify Category ==========");
                List<(int, string)> categories = categoryManager.ViewCategories();

                Console.WriteLine("Select a category to modify:");
                for (int i = 0; i < categories.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {categories[i].Item2}");
                }

                Console.Write("Enter the number of the category to modify: ");
                int categoryChoice = Convert.ToInt32(Console.ReadLine());

                if (categoryChoice >= 1 && categoryChoice <= categories.Count)
                {
                    var selectedCategory = categories[categoryChoice - 1];
                    string categoryName = selectedCategory.Item2;

                    Console.WriteLine($"Selected category: {categoryName}");
                    Console.WriteLine("1. Change Category Name");
                    Console.WriteLine("2. Add New Question");
                    Console.WriteLine("3. Delete Category");

                    Console.Write("Enter your choice: ");
                    int actionChoice = Convert.ToInt32(Console.ReadLine());

                    switch (actionChoice)
                    {
                        case 1:
                            Console.Write("Enter the new category name: ");
                            string newCategoryName = Console.ReadLine();
                            categoryManager.ChangeCategoryName(categoryName, newCategoryName);
                            Console.WriteLine($"Category name changed to {newCategoryName}.");
                            break;
                        case 2:
                            AddQuestionToCategory(categoryName);
                            break;
                        case 3:
                            DeleteCategory(categoryName);
                            break;
                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid category choice.");
                }
            }
            else
            {
                Console.WriteLine("You do not have permission to perform this action.");
            }
        }

        static string GetCurrentUserStatus()
        {
            if (currentUser != null)
            {
                if (currentUser.Username == "Guest")
                {
                    return "Signed in as Guest (Your quiz stats won't be tracked until you sign in)";
                }
                else
                {
                    return "Signed in as " + currentUser.Username;
                }
            }
            return "Not signed in";
        }
        static void AddQuestionToCategory(string categoryName)
        {
          
            int categoryId = categoryManager.GetCategoryIdByName(categoryName);

            if (categoryId != -1)
            {
                Console.WriteLine("Enter a question:");
                string question = Console.ReadLine() ?? string.Empty;

                Console.WriteLine("Enter the correct answer:");
                string correctAnswer = Console.ReadLine() ?? string.Empty;

                Console.WriteLine("Enter two incorrect answers, separated by a comma:");
                string[] incorrectAnswers = (Console.ReadLine() ?? string.Empty).Split(',');

         
                categoryManager.AddQuestionToCategory(categoryId, question, correctAnswer, incorrectAnswers);

                Console.WriteLine("Question added to the category successfully.");
            }
            else
            {
                Console.WriteLine("Invalid category name.");
            }
        }
        public static void DeleteCategory(string categoryName)
        {
            try
            {
                string query = "DELETE FROM Categories WHERE CategoryName = @CategoryName";
                SQLiteParameter[] parameters = { new SQLiteParameter("@CategoryName", categoryName) };

                bool result = databaseManager.ExecuteNonQuery(query, parameters);

                if (result)
                {
                    Console.WriteLine($"Category '{categoryName}' deleted successfully.");
                    categoryManager.LoadCategoriesFromDatabase();
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