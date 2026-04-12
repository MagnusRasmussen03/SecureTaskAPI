# SecureTaskAPI 🛡️
[![CI Pipeline](https://github.com/MagnusRasmussen03/SecureTaskAPI/actions/workflows/ci.yml/badge.svg)](https://github.com/MagnusRasmussen03/SecureTaskAPI/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED)
![License](https://img.shields.io/badge/license-MIT-green)

# SecureTaskAPI 🛡️

A task management REST API built with C# and .NET 10, PostgreSQL, Docker, and GitHub Actions CI/CD pipeline. Built as a hands-on DevSecOps learning project.

## Tech Stack

- **Language:** C# / .NET 10
- **Database:** PostgreSQL 16
- **ORM:** Entity Framework Core
- **Containerization:** Docker & Docker Compose
- **CI/CD:** GitHub Actions

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run the project locally

1. Clone the repository
```bash
   git clone https://github.com/MagnusRasmussen03/SecureTaskAPI.git
   cd SecureTaskAPI
```

2. Start the database
```bash
   docker-compose up
```

3. Apply database migrations
```bash
   cd SecureTaskAPI
   dotnet ef database update
```

4. Start the API
```bash
   dotnet run
```

The API is now running on `http://localhost:5274`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/tasks` | Get all tasks |
| GET | `/tasks/{id}` | Get a specific task |
| POST | `/tasks` | Create a new task |
| PUT | `/tasks/{id}` | Update a task |
| DELETE | `/tasks/{id}` | Delete a task |

### Example Request

**Create a new task:**
```json
POST /tasks
{
    "title": "Learn Docker",
    "isCompleted": false
}
```

**Response:**
```json
{
    "id": 1,
    "title": "Learn Docker",
    "isCompleted": false
}
```

## CI/CD Pipeline

This project uses GitHub Actions for continuous integration. The pipeline automatically triggers on every push to `main` and:

- ✅ Restores all dependencies
- ✅ Builds the project
- ✅ Reports success or failure

## Project Structure
SecureTaskAPI/
├── .github/
│   └── workflows/
│       └── ci.yml          # GitHub Actions pipeline
├── SecureTaskAPI/
│   ├── Migrations/         # EF Core database migrations
│   ├── AppDbContext.cs     # Database context
│   ├── TaskItem.cs         # Task model
│   ├── Program.cs          # API endpoints
│   └── appsettings.json    # Configuration
├── docker-compose.yml      # Database container
└── README.md

## What I Learned

- Building a REST API with ASP.NET Core and C#
- Database management with PostgreSQL and Entity Framework Core
- Containerization with Docker and Docker Compose
- Setting up CI/CD pipelines with GitHub Actions
- DevSecOps principles and practices