# Quiz App README

## Introduction
This is our Quiz App. This application offers a dynamic way to test your knowledge across various categories.

## Getting Started

### Initial Menu
Upon launching the app, you'll be greeted with the initial menu offering these options:
- **Sign In:** Existing users can sign in with their credentials.
- **Create Account:** New users can create an account for a personalized experience.
- **Play as Guest:** Play quizzes without saving your progress.
- **Exit Application:** Close the app.

### Administrator Access
- **Admin Login:** Use username `admin` and password `password` for admini access.
- **Admin Features:** Admins can modify categories, add or edit questions, and manage the database.

### Database
Our app utilizes SQLite for data management, ensuring your progress and stats are securely stored and easily retrievable.

## Playing the Quiz

### User Experience
Both guests and registered users can play quizzes. Registered users will have their progress tracked.

### Category Selection
Choose from various categories for a customized quiz experience.

### Question Distribution
We have tried to ensures a balanced mix of questions, but there might be some issues with the categories we have added that do not have any questions.

## Stats Tracking

### Data Storage
User stats are stored in SQLite, under `UserQuizStats`.
We also store Users with username and password, we did not focus on all the security measures for this task, or we would have hashed it.

### Performance Review
View your success percentage in different categories and track your improvement over time.

## Admin Features

### Direct Database Access
Admins can directly interact with the SQLite database.

### Category Management
Add, edit, or delete categories and questions from the console.