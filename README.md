# 🪺 CollabNest

> **Connect. Collaborate. Build Together.**

CollabNest is a full-stack web application that helps developers and creators find collaborators for their projects. Post your project, discover others, and send join requests — all in one place.

🔗 **Live Demo:** [collabnest-691774712889.us-central1.run.app](https://collabnest-691774712889.us-central1.run.app/)

---

## ✨ Features

- 🔐 **User Authentication** — Secure register & login with BCrypt password hashing
- 👤 **User Profiles** — Add your bio, skills, and view your posted projects
- 📁 **Project Listings** — Post projects with title, description, and required skills
- 🤝 **Collaboration Requests** — Send join requests with a message; accept or reject them
- 🔑 **Password Reset** — Secure email-based password reset with expiring tokens
- 🔔 **Toast Notifications** — Real-time success/error feedback throughout the app
- 📱 **Responsive Design** — Works seamlessly on desktop and mobile

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core MVC (.NET 10) |
| **Database** | SQLite via Entity Framework Core 10 |
| **Authentication** | Session-based + BCrypt.Net password hashing |
| **Email** | SMTP (Gmail-compatible) |
| **Frontend** | Razor Views, jQuery, Bootstrap |
| **Deployment** | Docker + Google Cloud Run |

---

## 📁 Project Structure

```
CollabNest/
├── Controllers/
│   ├── AccountController.cs      # Register, Login, Profile, Password Reset
│   ├── DashboardController.cs    # User dashboard
│   ├── HomeController.cs         # Landing page & project browsing
│   └── ProjectController.cs     # Create, View, Delete projects & requests
├── Models/
│   └── Models.cs                 # User, Project, CollabRequest, PasswordResetToken
├── ViewModels/
│   └── ViewModels.cs             # Form view models
├── Views/
│   ├── Account/                  # Login, Register, Profile, Password Reset views
│   ├── Dashboard/                # User dashboard
│   ├── Home/                     # Landing page
│   ├── Project/                  # Create & Details views
│   └── Shared/                   # Layout, Toast alerts
├── Data/
│   └── AppDbContext.cs           # EF Core database context
├── wwwroot/                      # Static assets (CSS, JS, icons)
├── Program.cs                    # App configuration & middleware
├── Dockerfile                    # Container configuration
└── appsettings.json              # App settings (SMTP, DB)
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git

### Run Locally

```bash
# 1. Clone the repository
git clone https://github.com/your-username/CollabNest.git
cd CollabNest

# 2. Configure email settings in appsettings.json
# (see Email Configuration section below)

# 3. Run the application
dotnet run

# 4. Open in browser
# https://localhost:5001
```

The SQLite database (`collabnest.db`) is automatically created on first run.

---

## ⚙️ Email Configuration

To enable password reset emails, update `appsettings.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPass": "your-app-password",
    "From": "your-email@gmail.com",
    "FromName": "CollabNest"
  }
}
```

> **Note:** For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833) instead of your main password.

---

## 🐳 Run with Docker

```bash
# Build the image
docker build -t collabnest .

# Run the container
docker run -p 8080:8080 collabnest

# Open in browser
# http://localhost:8080
```

---

## 🗄️ Database Models

```
User
├── Id, Name, Email, PasswordHash
├── Bio, Skills
└── Projects[], SentRequests[]

Project
├── Id, Title, Description, RequiredSkills
├── CreatedAt
└── Owner (User), CollabRequests[]

CollabRequest
├── Id, Message
├── Status (Pending / Accepted / Rejected)
└── Project, Sender (User)

PasswordResetToken
├── Id, Token, ExpiresAt, IsUsed
└── User
```

---

## 🌐 Deployment

This app is deployed on **Google Cloud Run** using Docker.

```bash
# Build and push to Google Container Registry
docker build -t gcr.io/YOUR_PROJECT_ID/collabnest .
docker push gcr.io/YOUR_PROJECT_ID/collabnest

# Deploy to Cloud Run
gcloud run deploy collabnest \
  --image gcr.io/YOUR_PROJECT_ID/collabnest \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated
```

---

## 🔒 Security Features

- Passwords hashed with **BCrypt** (industry standard)
- Password reset tokens are **single-use** and **expire in 1 hour**
- Existing tokens **invalidated** when a new reset is requested
- Email enumeration **prevented** — same response whether email exists or not
- Session cookies set to **HttpOnly**
- Project ownership **verified** before delete or request management

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

---

## 👨‍💻 Author

**Malik Muhammad Ahmad**

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

---

<p align="center">Built with ❤️ using ASP.NET Core</p>
