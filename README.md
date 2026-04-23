# TaskManager

TaskManager is an ASP.NET Core MVC web application inspired by Trello and Kanban workflow. The system allows users to create boards, manage lists, track tasks, and monitor project progress with a dashboard.

## Main Features

- Authentication and authorization with ASP.NET Core Identity
- Two roles: `Admin` and `User`
- CRUD operations for boards, lists, and tasks
- Drag-and-drop Kanban board
- Task fields: title, description, priority, deadline, assignee, status
- Dashboard statistics with LINQ:
  - completed vs pending tasks
  - top users with the most completed tasks this week
  - deadline alerts for upcoming tasks
- EF Core Code-First migrations
- Seeded demo accounts and sample board data
- Client-side and server-side validation with Data Annotations

## Technologies

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server / LocalDB
- ASP.NET Core Identity
- Razor Views
- Bootstrap 5

## Project Structure

- `Models/Entities` - database entities
- `Models/DTOs` - transfer models for UI-facing data
- `ViewModels` - form and page models
- `Services` - business logic and LINQ queries
- `Repositories` - data access abstraction for boards
- `Controllers` - MVC controllers
- `Views` - Razor UI
- `Data/Seeding` - roles, users, and demo data

## Default Accounts

- Admin:
  - Email: `admin@taskmanager.local`
  - Password: `Admin123!`
- User:
  - Email: `user@taskmanager.local`
  - Password: `User123!`

## Notes

- The current default connection string uses `LocalDB`. If your machine uses `SQLEXPRESS`, replace the server name in `appsettings.json`.

