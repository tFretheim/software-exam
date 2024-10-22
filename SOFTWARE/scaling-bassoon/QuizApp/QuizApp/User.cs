using System;

namespace QuizApp
{
    public class User
    {
        public int UserID { get; private set; }
        public string Username { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public bool IsAdmin { get; private set; }

        public User(int userID, string username, bool isAdmin)
        {
            UserID = userID;
            Username = username;
            IsLoggedIn = false;
            IsAdmin = isAdmin;
        }

        public void SignIn()
        {
            IsLoggedIn = true;
        }

        public void SignOut()
        {
            IsLoggedIn = false;
        }
    }
}