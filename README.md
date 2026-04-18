# GameReviews

GameReviews is a web application built with ASP.NET Core MVC that allows users to browse video games, create reviews, and request game additions through a role-based system.

---

# Features

### All Users
- Browse and view detailed information about games
- Filter games by genre and search by title or description
- View average rating based on user reviews

### Registered Users
- Create reviews with rating (1–10) and comment
- Edit and delete own reviews
- Add games to personal library
- Send a request to become a Game Dev
- View the status of their Game Dev request

### Game Dev
- All registered user features
- Submit a request to add a new game to the platform
- View the status of their submitted game requests

### Admin
- Review and approve/deny Game Dev role requests
- Review and approve/deny game addition requests
- Approved games are automatically added to the library
- Delete any game from the platform
- Delete any review
- Manage genres

---

# Technologies Used

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server (LocalDB)
- Bootstrap 5
- ASP.NET Identity

---

# Default Admin Account

- **Email:** admin@game.local
- **Password:** Admin123!
