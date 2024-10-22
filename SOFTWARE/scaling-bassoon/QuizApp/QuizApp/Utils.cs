using System;

namespace QuizApp
{
    public static class Utils
    {
        public static void PauseAndClear(int milliseconds = 1000)
        {
            Thread.Sleep(milliseconds); // Pause for set milliseconds, makes it possible to put different values in code
            Console.Clear();    // Clear the console
        }

        public static void ClearConsole()
        {
            Console.Clear();
        }

        public static void WaitForEnter()
        {
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }
    }
}