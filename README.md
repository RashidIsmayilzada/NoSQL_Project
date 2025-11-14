# NoSQL Project - Incident Management System

A full-featured IT Help Desk / Service Desk Ticket Management System built with ASP.NET Core 9.0 and MongoDB.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technologies](#technologies)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [Usage](#usage)
- [API Endpoints](#api-endpoints)
- [Authentication & Authorization](#authentication--authorization)
- [Screenshots](#screenshots)
- [Contributing](#contributing)
- [License](#license)

## Overview

This project is an **Incident Management System** designed for IT service desk operations. It provides a comprehensive platform for managing support tickets, employee profiles, and service desk workflows. The application uses MongoDB for data persistence and follows the Repository and Service pattern for clean architecture.

**Key Characteristics:**
- ASP.NET Core MVC for web interface with Razor views
- REST API endpoints for programmatic access
- MongoDB (NoSQL) for flexible document storage
- Role-based access control (Regular users vs ServiceDesk)
- Real-time dashboard analytics
- Email notifications for password resets

## Features

### Ticket Management
- **Create Tickets**: Users can report incidents with detailed information (type, priority, description, deadline)
- **View Tickets**:
  - Regular users see "My Tickets" (tickets they reported)
  - ServiceDesk sees both "My Tickets" (assigned to them) and "All Tickets"
- **Edit Tickets**: Users can edit their own tickets; ServiceDesk can edit any ticket
- **Assign Tickets**: ServiceDesk can assign tickets to specific ServiceDesk employees
- **Quick Assign**: ServiceDesk "Assign to Me" feature for fast ticket claiming
- **Close Tickets**: ServiceDesk can mark tickets as Closed
- **Search Tickets**: Full-text search with AND/OR operators on title and description
- **Ticket Attributes**:
  - **Type**: Hardware, Network, Software, Access, Other
  - **Priority**: Critical, High, Medium, Low
  - **Status**: Open, InProgress, OnHold, Resolved, Closed
  - **Deadline**: ISO-8601 formatted deadline tracking

### Employee Management (ServiceDesk Only)
- Create and manage employee profiles
- View all employees with their ticket counts
- View detailed employee information with reported tickets
- Enable/disable employee accounts
- Password management and reset functionality
- Role assignment (Regular / ServiceDesk)

### Dashboard Analytics
- **Status Breakdown**: Visual donut chart showing ticket distribution by status
- **Total Ticket Count**: Overall ticket statistics
- **Open Ticket Count**: Active tickets needing attention
- **Overdue Detection**: Automatic identification of tickets past their deadline
- **Scoped Views**: Different analytics for ServiceDesk (all tickets) vs Regular users (my tickets)

### Authentication & Security
- Cookie-based authentication with 8-hour sessions (sliding expiration)
- BCrypt password hashing with work factor 10
- Role-based access control
- Forgot password functionality with email token reset
- Claim-based authorization storing user ID, name, role, and email
- HTTPS redirection and HSTS headers

### Email Integration
- SMTP email sending via Gmail
- Password reset token delivery
- Configurable email templates

## Technologies

### Core Framework
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core 9.0** - Web application framework
- **C# 12** - With nullable reference types enabled

### NuGet Packages
- **MongoDB.Driver** (v3.5.0) - MongoDB database driver
- **MongoDB.Bson** (v3.5.0) - BSON serialization
- **BCrypt.Net-Next** (v4.0.3) - Secure password hashing
- **DotNetEnv** (v3.1.1) - Environment variable management
- **Microsoft.VisualStudio.Web.CodeGeneration.Design** (v9.0.0) - Code generation tools

### Frontend
- **Razor View Engine** - Server-side rendering
- **Bootstrap** - Responsive UI framework
- **Custom CSS & JavaScript** - Enhanced user experience

### Database
- **MongoDB** - NoSQL document database for flexible data storage

## Prerequisites

Before running this application, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (Community or Atlas)
- A code editor (Visual Studio 2022, VS Code, or Rider)
- SMTP email account (Gmail recommended for password reset functionality)

## Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd NoSQL_Project
```

### 2. Set Up MongoDB

**Option A: Local MongoDB**
- Install MongoDB Community Edition
- Start MongoDB service
- Create a database named `IncidentManagement`

**Option B: MongoDB Atlas**
- Create a free cluster at [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
- Create a database named `IncidentManagement`
- Whitelist your IP address
- Get your connection string

### 3. Configure Environment Variables

Create a `.env` file in the project root:

```env
Mongo:ConnectionString=mongodb://localhost:27017
# OR for MongoDB Atlas:
# Mongo:ConnectionString=mongodb+srv://<username>:<password>@<cluster>.mongodb.net/

Email:Password=your_gmail_app_password
```

**Note**: For Gmail, you need to generate an [App Password](https://support.google.com/accounts/answer/185833) if 2FA is enabled.

### 4. Update appsettings.json

The `appsettings.json` file contains additional configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  "Mongo": {
    "Database": "IncidentManagement"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your_email@gmail.com",
    "EnableSsl": true
  }
}
```

### 5. Restore Dependencies

```bash
dotnet restore
```

### 6. Build the Application

```bash
dotnet build
```

### 7. Run the Application

```bash
dotnet run
```

The application will be available at:
- HTTPS: `https://localhost:7027`
- HTTP: `http://localhost:5009`

## Configuration

### MongoDB Configuration

The MongoDB connection is configured in two places:

1. **appsettings.json**: Database name
```json
"Mongo": {
  "Database": "IncidentManagement"
}
```

2. **.env file**: Connection string
```env
Mongo:ConnectionString=mongodb://localhost:27017
```

### Email Configuration

Email settings in **appsettings.json**:
```json
"Email": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UserName": "your_email@gmail.com",
  "EnableSsl": true
}
```

Email password in **.env file**:
```env
Email:Password=your_app_password
```

### Authentication Configuration

Cookie authentication is configured in `Program.cs`:
- **Cookie Name**: "MyCookie"
- **Login Path**: /Login/Login
- **Expiration**: 8 hours (sliding)
- **Session Timeout**: 120 minutes

## Usage

### First Time Setup

1. **Start the Application**: Run `dotnet run` and navigate to `https://localhost:7027`

2. **Create First ServiceDesk User**: Since there's no admin panel, you'll need to create a ServiceDesk user manually in MongoDB:

```javascript
// MongoDB Shell or Compass
db.Employee.insertOne({
  IsDisabled: false,
  Name: {
    FirstName: "Admin",
    LastName: "User"
  },
  Role: "ServiceDesk",
  Password: "$2a$10$...",  // Generate BCrypt hash for your password
  contactInfo: {
    Email: "admin@example.com",
    Phone: "123-456-7890",
    Location: "IT Department"
  },
  ReportedTickets: []
})
```

3. **Login**: Go to `/Login/Login` and sign in with your ServiceDesk credentials

### User Workflows

#### As a Regular User:

1. **Login** � Dashboard shows your reported tickets
2. **Create Ticket** � Report new incident
3. **View My Tickets** � See all your reported tickets
4. **Edit Ticket** � Update ticket details (if not closed)
5. **Search** � Find specific tickets by keywords

#### As a ServiceDesk User:

1. **Login** � Dashboard shows all tickets analytics
2. **View All Tickets** � See system-wide tickets
3. **View My Tickets** � See tickets assigned to you
4. **Assign Tickets** � Assign tickets to team members
5. **Assign to Me** � Quick-claim tickets
6. **Close Tickets** � Mark tickets as resolved/closed
7. **Manage Employees** � Create/edit/disable user accounts
8. **Search** � Advanced search across all tickets

### Common Operations

#### Creating a Ticket

1. Navigate to **Tickets** � **Create Ticket**
2. Fill in the form:
   - **Title**: Brief description
   - **Type**: Select from dropdown
   - **Priority**: Select urgency level
   - **Deadline**: Set due date
   - **Description**: Detailed explanation
3. Click **Submit**

#### Assigning a Ticket

1. Go to **Ticket Details**
2. Click **Assign** button
3. Select ServiceDesk employee from dropdown
4. Click **Assign**

OR use **Assign to Me** for quick self-assignment

#### Searching Tickets

Use the search bar with keywords. The system supports:
- **AND operator**: Space-separated words (e.g., "network printer" finds tickets with both words)
- **OR operator**: Use `|` (e.g., "network|printer" finds tickets with either word)
- **Scope**: "My Tickets" or "All Tickets" (ServiceDesk only)

## API Endpoints

The application provides REST API endpoints under `/api/rashid`:

### Authentication

```
POST /api/rashid/login
Body: { "email": "user@example.com", "password": "password" }
Response: User object with authentication cookie
```

### Dashboard

```
GET /api/rashid/dashboard
Response: Dashboard statistics and status breakdown
```

### Employees

```
GET /api/rashid/employees
Response: List of all employees

GET /api/rashid/employees/{id}
Response: Employee details

GET /api/rashid/employees/with-tickets
Response: Employees with their associated tickets

POST /api/rashid/employees
Body: Employee creation data
Response: Created employee

PUT /api/rashid/employees/{id}
Body: Updated employee data
Response: Updated employee
```

### Tickets

```
GET /api/rashid/tickets
Response: All tickets

GET /api/rashid/tickets/{id}
Response: Ticket details

GET /api/rashid/tickets/my
Response: Tickets reported by current user

GET /api/rashid/tickets/assigned-to-me
Response: Tickets assigned to current user

POST /api/rashid/tickets
Body: Ticket creation data
Response: Created ticket

PUT /api/rashid/tickets/{id}
Body: Updated ticket data
Response: Updated ticket

POST /api/rashid/tickets/{id}/assign
Body: { "assigneeUserId": "employee_id" }
Response: Updated ticket

POST /api/rashid/tickets/{id}/close
Response: Closed ticket

GET /api/rashid/tickets/search?q=query&scope=MyTickets
Response: Search results
```

### Password Reset

```
POST /api/rashid/forgot-password
Body: { "email": "user@example.com" }
Response: Success message (email sent)

POST /api/rashid/reset-password
Body: { "token": "reset_token", "newPassword": "password", "confirmPassword": "password" }
Response: Success message
```

## Authentication & Authorization

### Cookie-Based Authentication

The application uses ASP.NET Core Cookie Authentication:
- **Cookie Name**: "MyCookie"
- **Expiration**: 8 hours with sliding expiration
- **Secure**: HTTPS only in production
- **SameSite**: Strict

### Claims Stored in Cookie

```csharp
new Claim("UserId", employee.Id)
new Claim(ClaimTypes.Name, employee.Name.ToString())
new Claim(ClaimTypes.Role, employee.Role.ToString())
new Claim(ClaimTypes.Email, employee.ContactInfo.Email)
```

### Authorization Levels

1. **Anonymous**: Login, Forgot Password pages
2. **[Authorize]**: All authenticated users (Regular + ServiceDesk)
3. **ServiceDesk Only**: Employee management, ticket assignment, close tickets

### Session Management

- **Session Timeout**: 120 minutes (sliding)
- **Session Storage**: In-memory (can be configured for distributed cache)
- **Auto Logout**: After timeout or manual logout

## Screenshots

### Dashboard
The dashboard provides an overview of ticket statistics with a donut chart visualization showing status breakdown.

### Ticket Management
Create, view, edit, and assign tickets with a clean and intuitive interface.

### Employee Management
ServiceDesk users can manage employee profiles, view ticket counts, and control account status.

## Contributing

### Development Guidelines

1. **Code Style**: Follow C# naming conventions and use nullable reference types
2. **Comments**: Add single-line comments (`//`) above all classes and methods
3. **Architecture**: Maintain separation of concerns (Controllers � Services � Repositories)
4. **Async/Await**: Use async methods for all database operations
5. **Error Handling**: Implement proper exception handling and logging
6. **Testing**: Write unit tests for services and repositories

### Making Changes

1. Create a feature branch: `git checkout -b feature/your-feature-name`
2. Make your changes following the coding standards
3. Test thoroughly
4. Commit with descriptive messages
5. Push and create a pull request

### Branching Strategy

- **main**: Production-ready code
- **develop**: Integration branch
- **feature/***: Feature development
- **bugfix/***: Bug fixes
- **hotfix/***: Production hotfixes

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

## Additional Resources

### MongoDB Resources
- [MongoDB .NET Driver Documentation](https://www.mongodb.com/docs/drivers/csharp/)
- [MongoDB University](https://university.mongodb.com/) - Free courses

### ASP.NET Core Resources
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

### Deployment
For deploying this application:
- **Azure App Service** with MongoDB Atlas
- **Docker** containerization
- **AWS Elastic Beanstalk** with MongoDB Atlas

---

## Support

For issues, questions, or contributions, please:
1. Check existing issues in the repository
2. Create a new issue with detailed information
3. Contact the development team

---

## Changelog

### Recent Updates
-  Changed employee list retrieval to show only ServiceDesk employees for ticket assignment
-  Refactored employee retrieval and ticket management
-  Fixed minor ticket editing issues
-  Enhanced employee authentication and password change functionality
-  Applied visual adjustments based on team feedback

---

**Built with d using ASP.NET Core and MongoDB**
