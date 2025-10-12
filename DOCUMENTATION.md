# Incident Management System - Complete Documentation

## Table of Contents
1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Architecture Overview](#architecture-overview)
4. [Database Design](#database-design)
5. [Application Structure](#application-structure)
6. [Feature Documentation](#feature-documentation)
7. [Why CRUD Operations Were Needed](#why-crud-operations-were-needed)
8. [Key Implementation Details](#key-implementation-details)
9. [Best Practices Applied](#best-practices-applied)
10. [Setup and Configuration](#setup-and-configuration)

---

## Project Overview

### Purpose
The Incident Management System is a comprehensive web application designed to manage IT support tickets and employee records. It provides a centralized platform for tracking incidents, managing employee information, and monitoring ticket statuses through an intuitive dashboard interface.

### Core Functionality
- **Ticket Management**: Create, read, update, and delete support tickets
- **Employee Management**: Manage employee records and their associated tickets
- **Dashboard Analytics**: Real-time statistics on ticket status and deadlines
- **Relationship Tracking**: Link tickets to employees (reporters and handlers)

### Business Goals
1. **Streamline Incident Resolution**: Track and manage IT incidents efficiently
2. **Improve Response Times**: Monitor overdue tickets and unresolved issues
3. **Resource Management**: Track which employees are handling which tickets
4. **Data-Driven Decisions**: Provide dashboard metrics for management insights

---

## Technology Stack

### Backend Technologies
- **Framework**: ASP.NET Core 9.0 (MVC Pattern)
- **Language**: C# with .NET 9.0
- **Database**: MongoDB (NoSQL Document Database)
- **Driver**: MongoDB.Driver v3.5.0
- **Configuration**: DotNetEnv v3.1.1 (Environment Variable Management)

### Frontend Technologies
- **View Engine**: Razor Views (.cshtml)
- **CSS Framework**: Bootstrap 5
- **UI Components**: Custom SVG graphics for dashboard visualizations
- **Templating**: ASP.NET Core MVC with Razor syntax

### Development Tools
- **Runtime**: .NET 9.0
- **Package Manager**: NuGet
- **IDE**: Compatible with Visual Studio, Visual Studio Code, or JetBrains Rider

---

## Architecture Overview

### Design Pattern: Repository-Service-Controller (Clean Architecture)

The application follows a layered architecture pattern that promotes separation of concerns, maintainability, and testability:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  (Controllers + Views)                  │
│  - HomeController                       │
│  - TicketController                     │
│  - EmployeeController                   │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Service Layer                   │
│  (Business Logic)                       │
│  - ITicketService / TicketService       │
│  - IEmployeeService / EmployeeService   │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Repository Layer                │
│  (Data Access)                          │
│  - ITicketRepository / TicketRepository │
│  - IEmployeeRepository / EmployeeRepo   │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Data Layer                      │
│  (MongoDB Database)                     │
│  - Employee Collection                  │
│  - Ticket Collection                    │
└─────────────────────────────────────────┘
```

### Layer Responsibilities

#### 1. **Presentation Layer (Controllers + Views)**
- **Location**: `/Controllers` and `/Views`
- **Responsibility**: Handle HTTP requests, user input validation, and render responses
- **Components**:
  - `HomeController`: Dashboard and landing pages
  - `TicketController`: Full CRUD operations for tickets
  - `EmployeeController`: Full CRUD operations for employees

#### 2. **Service Layer (Business Logic)**
- **Location**: `/Services`
- **Responsibility**: Business logic, data transformation, and orchestration
- **Benefits**:
  - Decouples business logic from controllers
  - Enables business rule changes without modifying controllers
  - Provides a clean interface for unit testing

#### 3. **Repository Layer (Data Access)**
- **Location**: `/Repositories`
- **Responsibility**: Direct database operations and MongoDB aggregation pipelines
- **Benefits**:
  - Abstracts database implementation details
  - Makes it easy to switch databases in the future
  - Centralizes data access logic

#### 4. **Data Layer (MongoDB)**
- **Responsibility**: Data persistence and storage
- **Collections**: `Employee`, `Ticket`

---

## Database Design

### Database: IncidentManagement

### Collections

#### 1. **Employee Collection**

**Schema Structure:**
```json
{
  "_id": ObjectId,
  "EmployeeId": Number,
  "Name": {
    "FirstName": String,
    "LastName": String
  },
  "Role": String (enum: "Regular" | "ServiceDesk"),
  "contactInfo": {
    "Email": String,
    "Phone": String,
    "Location": String
  },
  "ReportedTickets": Array<Ticket> (populated via aggregation)
}
```

**Indexes**:
- Primary: `_id` (ObjectId)
- Unique: `EmployeeId`

**Purpose**: Store employee information and track their role in the incident management system

#### 2. **Ticket Collection**

**Schema Structure:**
```json
{
  "_id": ObjectId,
  "TicketId": String,
  "Title": String (required),
  "Type": String (enum: "Hardware" | "Network" | "Software" | "Access" | "Other"),
  "Priority": String (enum: "Critical" | "High" | "Medium" | "Low"),
  "Status": String (enum: "Open" | "InProgress" | "OnHold" | "Resolved" | "Closed"),
  "Deadline": String (ISO date),
  "Description": String,
  "ReportedBy": ObjectId (reference to Employee._id),
  "HandeledBy": [
    {
      "employeeID": ObjectId (reference to Employee._id),
      "date": String (ISO date)
    }
  ]
}
```

**Indexes**:
- Primary: `_id` (ObjectId)
- Foreign Key: `ReportedBy` references `Employee._id`
- Array Foreign Keys: `HandeledBy[].employeeID` references `Employee._id`

**Purpose**: Store support tickets with relationships to employees who reported or are handling them

### Relationship Design

#### Employee-Ticket Relationship (One-to-Many)
- **Type**: One Employee can report many Tickets
- **Implementation**: `Ticket.ReportedBy` stores Employee's `_id` (ObjectId reference)
- **Population Strategy**: MongoDB `$lookup` aggregation pipeline

#### Ticket-Handler Relationship (Many-to-Many)
- **Type**: Multiple Employees can handle one Ticket, and one Employee can handle many Tickets
- **Implementation**: `Ticket.HandeledBy` array stores embedded documents with Employee references
- **Population Strategy**: Aggregation pipeline with array unwinding

### Why MongoDB NoSQL?

1. **Flexible Schema**: Easy to add new fields to tickets or employees without migrations
2. **Embedded Documents**: `Name` and `ContactInfo` are naturally embedded objects
3. **Complex Queries**: Aggregation pipeline allows powerful data transformations
4. **Scalability**: Horizontal scaling for growing ticket volumes
5. **JSON-Native**: Natural mapping between C# objects and BSON documents

---

## Application Structure

### Project File Organization

```
NoSQL_Project/
├── Controllers/
│   ├── HomeController.cs          # Dashboard and landing pages
│   ├── TicketController.cs        # Ticket CRUD operations
│   ├── EmployeeController.cs      # Employee CRUD operations
│   └── LoginController.cs         # Authentication (if used)
├── Models/
│   ├── Employee.cs                # Employee entity model
│   ├── Ticket.cs                  # Ticket entity model
│   ├── DashboardViewModel.cs      # Dashboard statistics DTO
│   ├── ErrorViewModel.cs          # Error handling model
│   └── Enums/
│       ├── RoleType.cs            # Employee role enumeration
│       ├── TicketType.cs          # Ticket type enumeration
│       ├── TicketPriority.cs      # Priority levels
│       └── TicketStatus.cs        # Ticket status enumeration
├── Repositories/
│   ├── Interfaces/
│   │   ├── IEmployeeRepository.cs # Employee data access contract
│   │   └── ITicketRepository.cs   # Ticket data access contract
│   ├── EmployeeRepository.cs      # Employee data implementation
│   └── TicketRepository.cs        # Ticket data implementation
├── Services/
│   ├── Interfaces/
│   │   ├── IEmployeeService.cs    # Employee business logic contract
│   │   └── ITicketService.cs      # Ticket business logic contract
│   ├── EmployeeService.cs         # Employee business implementation
│   └── TicketService.cs           # Ticket business implementation
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml           # Dashboard with statistics
│   │   └── Privacy.cshtml         # Privacy page
│   ├── Ticket/
│   │   ├── Index.cshtml           # Ticket list view
│   │   ├── Details.cshtml         # Ticket details view
│   │   ├── Edit.cshtml            # Ticket edit form
│   │   └── Delete.cshtml          # Ticket delete confirmation
│   ├── Employee/
│   │   └── Index.cshtml           # Employee list view
│   └── Shared/
│       ├── _Layout.cshtml         # Main layout template
│       ├── Error.cshtml           # Error page template
│       └── _ValidationScriptsPartial.cshtml
├── Program.cs                     # Application entry point & DI config
├── appsettings.json               # Configuration (DB name)
└── NoSQL_Project.csproj           # Project dependencies
```

---

## Feature Documentation

### 1. Dashboard (Home/Index)

**Purpose**: Provide at-a-glance metrics for incident management

**Features**:
- **Total Tickets Count**: Shows overall ticket volume
- **Unresolved Tickets**: Displays tickets that are not Closed or Resolved
- **Past Deadline Tickets**: Highlights urgent tickets requiring immediate attention
- **Visual Indicators**: SVG circular progress bars for quick status assessment

**Implementation Details**:
- **Controller**: `HomeController.cs:19-31`
- **Service Method**: `TicketService.GetDashboardStatisticsAsync()` (line 46-55)
- **Repository Methods**:
  - `GetTotalTicketsCountAsync()` (TicketRepository.cs:129-132)
  - `GetUnresolvedTicketsCountAsync()` (TicketRepository.cs:134-141)
  - `GetTicketsPastDeadlineCountAsync()` (TicketRepository.cs:143-181)
- **View**: `Views/Home/Index.cshtml`

**Key Algorithm - Past Deadline Calculation**:
```csharp
// TicketRepository.cs:143-181
// 1. Fetch all tickets with Deadline and Status fields
// 2. Parse deadline strings to DateTime
// 3. Compare with current date
// 4. Exclude Closed and Resolved tickets
// 5. Count tickets where deadline < today
```

### 2. Ticket Management

#### 2.1 View All Tickets (Index)

**Purpose**: Display a comprehensive list of all tickets with key information

**Features**:
- Tabular display of all tickets
- Badge-based status visualization (color-coded)
- Priority indicators (Low=Secondary, Medium=Warning, High=Danger)
- Action buttons (Details, Edit, Delete)
- Empty state message when no tickets exist

**Implementation**:
- **Controller Method**: `TicketController.Index()` (line 22-42)
- **Repository Method**: `TicketRepository.GetAllTickets()` (line 17-53)
- **View**: `Views/Ticket/Index.cshtml`

**MongoDB Aggregation Pipeline**:
```javascript
[
  // Stage 1: Lookup ReportedBy employee
  {
    $lookup: {
      from: "Employee",
      localField: "ReportedBy",
      foreignField: "_id",
      as: "ReportedBy",
      pipeline: [
        {
          $project: {
            _id: 1, EmployeeId: 1, Name: 1,
            Role: 1, contactInfo: 1
            // ReportedTickets EXCLUDED to prevent infinite loop
          }
        }
      ]
    }
  },
  // Stage 2: Convert array to single object
  { $unwind: { path: "$ReportedBy", preserveNullAndEmptyArrays: true } }
]
```

**Why Aggregation Pipeline?**
- Populates employee data without creating circular references
- More efficient than multiple database calls
- Prevents infinite loops when employees have tickets and tickets have employees

#### 2.2 View Ticket Details

**Purpose**: Display complete information about a single ticket

**Features**:
- Full ticket information display
- Reporter employee details
- Handler employees list with handling dates
- Status and priority badges

**Implementation**:
- **Controller Method**: `TicketController.Details(string id)` (line 46-89)
- **Repository Method**: `TicketRepository.GetTicketById(string id)` (line 55-94)
- **View**: `Views/Ticket/Details.cshtml`

**Validation**:
1. Null/empty ID check
2. ObjectId format validation using `ObjectId.TryParse()`
3. Database existence check
4. Error handling for MongoDB exceptions

#### 2.3 Create New Ticket

**Purpose**: Add new support tickets to the system

**Features**:
- Form-based data entry
- Model validation
- Anti-forgery token protection
- Success/error feedback via TempData

**Implementation**:
- **GET Method**: `TicketController.Create()` (line 93-96) - Display form
- **POST Method**: `TicketController.Create(Ticket ticket)` (line 101-136) - Process submission
- **Repository Method**: `TicketRepository.CreateTicket(Ticket ticket)` (line 96-99)
- **View**: `Views/Ticket/Edit.cshtml` (shared with Edit)

**Validation Flow**:
```
User Input → Model Binding → Model Validation → Service Layer → Repository → Database
     ↓              ↓                ↓                ↓              ↓           ↓
   Form      C# Properties    DataAnnotations    Business Logic   MongoDB   Success
                                                                   Insert    Response
```

#### 2.4 Update Existing Ticket

**Purpose**: Modify ticket information, status, and assignments

**Features**:
- Pre-filled form with existing data
- Support for status updates
- Handler assignment management
- ReportedBy employee preservation

**Implementation**:
- **GET Method**: `TicketController.Edit(string id)` (line 140-183) - Load ticket
- **POST Method**: `TicketController.Edit(Ticket ticket, string reportedById)` (line 188-261)
- **Repository Method**: `TicketRepository.UpdateTicket(string id, Ticket ticket)` (line 101-122)
- **View**: `Views/Ticket/Edit.cshtml`

**Special Handling**:
```csharp
// TicketController.cs:215-223
// Remove ModelState errors for properties not bound from form
ModelState.Remove("ReportedBy");

// Remove errors for nested Employee objects in HandledBy
var handledByKeys = ModelState.Keys
    .Where(k => k.StartsWith("HandledBy[") && k.Contains(".Employee"))
    .ToList();
foreach (var key in handledByKeys) {
    ModelState.Remove(key);
}
```

**MongoDB Update Strategy**:
```csharp
// TicketRepository.cs:104-119
// Use Builders pattern for field-specific updates
var update = Builders<Ticket>.Update
    .Set(t => t.TicketId, ticket.TicketId)
    .Set(t => t.Title, ticket.Title)
    // ... more fields ...

// Store only ObjectId for ReportedBy (not full Employee object)
if (ticket.ReportedBy != null) {
    var reportedByObjectId = new ObjectId(ticket.ReportedBy.Id);
    update = update.Set("ReportedBy", reportedByObjectId);
}
```

#### 2.5 Delete Ticket

**Purpose**: Remove tickets from the system with confirmation

**Features**:
- Two-step deletion (confirmation page → deletion)
- Display ticket details before deletion
- Safe deletion with proper error handling

**Implementation**:
- **GET Method**: `TicketController.Delete(string id)` (line 265-308) - Confirmation page
- **POST Method**: `TicketController.DeleteConfirmed(string id)` (line 313-350)
- **Repository Method**: `TicketRepository.DeleteTicket(string id)` (line 124-127)
- **View**: `Views/Ticket/Delete.cshtml`

### 3. Employee Management

#### 3.1 View All Employees

**Purpose**: Display list of all employees in the system

**Implementation**:
- **Controller Method**: `EmployeeController.Index()` (line 18-38)
- **Repository Method**: `EmployeeRepository.GetAllEmployees()` (line 18-21)
- **View**: `Views/Employee/Index.cshtml`

#### 3.2 View Employees with Tickets

**Purpose**: Show employees along with all tickets they've reported

**Features**:
- Aggregated view of employee-ticket relationships
- Useful for workload analysis

**Implementation**:
- **Controller Method**: `EmployeeController.EmployeesWithTickets()` (line 89-109)
- **Repository Method**: `EmployeeRepository.GetEmployeesWithTicket()` (line 23-38)

**MongoDB Aggregation**:
```javascript
[
  {
    $lookup: {
      from: "Ticket",
      localField: "_id",
      foreignField: "ReportedBy",
      as: "ReportedTickets"
    }
  }
]
```

#### 3.3 Employee CRUD Operations

Similar to Ticket CRUD with these methods:
- **Create**: `EmployeeController.Create()` (GET: line 113-116, POST: line 121-156)
- **Edit**: `EmployeeController.Edit()` (GET: line 160-203, POST: line 208-258)
- **Delete**: `EmployeeController.Delete()` (GET: line 262-305, POST: line 310-347)

---

## Why CRUD Operations Were Needed

### Business Justification

#### 1. **Create Operations**

**Why Needed**:
- **New Incident Reporting**: Employees need to report new IT issues
- **Employee Onboarding**: HR needs to add new employees to the system
- **Dynamic System**: Business grows, new tickets and employees are added continuously

**Business Impact**:
- Without Create: No way to start tracking new incidents
- With Create: Enables proactive incident management from the moment an issue arises

**Real-World Scenario**:
```
Employee reports: "My laptop won't connect to WiFi"
→ Service desk creates ticket via form
→ Ticket enters system with status "Open"
→ Can now be tracked, assigned, and resolved
```

#### 2. **Read Operations**

**Why Needed**:
- **Ticket Monitoring**: Service desk needs to see all open tickets
- **Individual Lookup**: View specific ticket details for troubleshooting
- **Dashboard Analytics**: Management needs aggregate statistics
- **Employee Lookup**: Find employee contact information

**Business Impact**:
- Without Read: No visibility into system state
- With Read: Complete transparency and informed decision-making

**Multiple Read Types**:
- **List View** (GetAll): See all tickets/employees
- **Detail View** (GetById): Deep dive into specific records
- **Aggregated View** (Dashboard): High-level metrics

#### 3. **Update Operations**

**Why Needed**:
- **Status Changes**: Ticket moves through lifecycle (Open → InProgress → Resolved → Closed)
- **Priority Adjustments**: High-priority issues escalated as needed
- **Reassignment**: Tickets moved between support staff
- **Information Correction**: Fix typos, update descriptions
- **Employee Information**: Contact details change, role promotions

**Business Impact**:
- Without Update: Data becomes stale and inaccurate
- With Update: System reflects current reality

**Real-World Scenario**:
```
Ticket created: Priority=Low, Status=Open
↓
Tech starts working: Status → InProgress
↓
Issue more serious than thought: Priority → Critical
↓
Solution found: Status → Resolved
↓
User confirms fix: Status → Closed
```

#### 4. **Delete Operations**

**Why Needed**:
- **Test Data Cleanup**: Remove dummy tickets created during testing
- **Duplicate Removal**: Delete accidentally created duplicates
- **Employee Offboarding**: Remove former employees from the system
- **Data Compliance**: GDPR/privacy requirements for data deletion
- **Mistake Correction**: Remove incorrectly created records

**Business Impact**:
- Without Delete: Database cluttered with invalid/test data
- With Delete: Clean, accurate dataset for reporting

**Real-World Scenarios**:
1. **Duplicate Prevention**: User clicks "Create" twice → two identical tickets → delete one
2. **Test Environment**: After training session → 50 test tickets → bulk delete
3. **Privacy Compliance**: Employee leaves company → delete personal data

### Technical Justification

#### Data Lifecycle Management
```
CREATE → READ → UPDATE → DELETE
  ↓       ↓       ↓        ↓
Birth   Use    Maintain  End
```

Without full CRUD:
- **Missing Create**: Cannot add new data
- **Missing Read**: Cannot verify or use data
- **Missing Update**: Data becomes outdated
- **Missing Delete**: Database grows indefinitely

#### CRUD vs. Business Operations Mapping

| Business Need | CRUD Operation | Example |
|--------------|---------------|---------|
| Report New Incident | Create | User submits help ticket |
| Check Ticket Status | Read | Service desk views ticket list |
| Assign Ticket to Tech | Update | Manager assigns ticket to employee |
| Remove Spam Ticket | Delete | Admin deletes fake/spam ticket |
| Update Contact Info | Update | Employee changes phone number |
| Onboard New Employee | Create | HR adds new hire to system |
| View Dashboard Stats | Read (Aggregated) | Manager views unresolved ticket count |
| Close Resolved Ticket | Update | Tech marks ticket as "Closed" |

---

## Key Implementation Details

### 1. Dependency Injection Configuration

**Location**: `Program.cs:14-44`

```csharp
// Singleton MongoClient (one instance for entire application)
builder.Services.AddSingleton<IMongoClient>(sp => {
    var conn = builder.Configuration["Mongo:ConnectionString"];
    return new MongoClient(MongoClientSettings.FromConnectionString(conn));
});

// Scoped IMongoDatabase (new instance per HTTP request)
builder.Services.AddScoped(sp => {
    var client = sp.GetRequiredService<IMongoClient>();
    var dbName = builder.Configuration["Mongo:Database"];
    return client.GetDatabase(dbName);
});

// Scoped Repositories and Services
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketService, TicketService>();
```

**Why This Design?**:
- **Singleton MongoClient**: MongoDB client is thread-safe and manages connection pooling internally
- **Scoped Database**: Each HTTP request gets its own database instance, preventing conflicts
- **Scoped Services/Repos**: Aligns with ASP.NET Core request lifecycle

### 2. Error Handling Strategy

**Comprehensive Try-Catch Blocks**:
```csharp
// TicketController.cs:24-42
try {
    _logger.LogInformation("Retrieving all tickets");
    var tickets = await _ticketService.GetAllTicketsAsync();
    return View(tickets);
}
catch (MongoException ex) {
    _logger.LogError(ex, "Database error while retrieving tickets");
    TempData["Error"] = "Unable to retrieve tickets. Please try again later.";
    return View("Error");
}
catch (Exception ex) {
    _logger.LogError(ex, "Unexpected error while retrieving tickets");
    TempData["Error"] = "An unexpected error occurred.";
    return View("Error");
}
```

**Error Handling Layers**:
1. **MongoDB-Specific Errors**: Catch `MongoException` for database issues
2. **General Errors**: Catch `Exception` for unexpected issues
3. **Logging**: All errors logged with context
4. **User Feedback**: User-friendly messages via `TempData`

### 3. Validation Strategy

**Multi-Layer Validation**:

1. **Model Validation** (Data Annotations):
```csharp
// Ticket.cs:19
[Required]
public string Title { get; set; } = "";
```

2. **Controller Validation** (Input Checking):
```csharp
// TicketController.cs:50-63
if (string.IsNullOrEmpty(id)) {
    _logger.LogWarning("Details called with null or empty ID");
    TempData["Error"] = "Ticket ID is required.";
    return RedirectToAction(nameof(Index));
}

if (!ObjectId.TryParse(id, out _)) {
    _logger.LogWarning("Invalid ticket ID format: {TicketId}", id);
    TempData["Error"] = "Invalid ticket ID format.";
    return RedirectToAction(nameof(Index));
}
```

3. **ModelState Validation**:
```csharp
// TicketController.cs:112-116
if (!ModelState.IsValid) {
    _logger.LogWarning("Invalid model state for ticket creation");
    return View(ticket);
}
```

### 4. MongoDB Aggregation Pipeline Usage

**Why Use Aggregation Instead of Simple Queries?**

**Problem**: Circular Reference Loop
```
Employee → ReportedTickets (array of Tickets)
  ↓
Ticket → ReportedBy (Employee object)
  ↓
Employee → ReportedTickets (array of Tickets)
  ↓
∞ Infinite loop!
```

**Solution**: Controlled Projection via Aggregation
```csharp
// TicketRepository.cs:22-43
var pipeline = new[] {
    new BsonDocument("$lookup", new BsonDocument {
        { "from", "Employee" },
        { "localField", "ReportedBy" },
        { "foreignField", "_id" },
        { "as", "ReportedBy" },
        { "pipeline", new BsonArray {
            new BsonDocument("$project", new BsonDocument {
                { "_id", 1 },
                { "EmployeeId", 1 },
                { "Name", 1 },
                { "Role", 1 },
                { "contactInfo", 1 }
                // ReportedTickets NOT included!
            })
        }}
    })
};
```

**Benefits**:
- **Prevents Infinite Loops**: Explicitly excludes nested arrays
- **Performance**: Fetches only needed fields
- **Single Query**: No N+1 query problem
- **Type Safety**: Still maps to C# objects

### 5. BSON Attribute Mapping

**Purpose**: Control how C# objects serialize to MongoDB BSON documents

```csharp
// Ticket.cs:9-14
[BsonIgnoreExtraElements]  // Ignore fields in DB not in C# model
public class Ticket {
    [BsonId]  // Mark as MongoDB primary key
    [BsonRepresentation(BsonType.ObjectId)]  // Store as ObjectId
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.String)]  // Store enum as string
    public TicketType Type { get; set; }
}
```

**Key Attributes**:
- `[BsonIgnoreExtraElements]`: Schema flexibility (ignore unknown fields)
- `[BsonId]`: Designates primary key field
- `[BsonElement("name")]`: Rename field in database
- `[BsonRepresentation(BsonType.String)]`: Store enums as readable strings
- `[BsonIgnore]`: Exclude property from database storage
- `[BsonIgnoreIfNull]`: Don't store if value is null

### 6. Asynchronous Programming

**All database operations use async/await**:

```csharp
// TicketService.cs:16-19
public async Task<List<Ticket>> GetAllTicketsAsync() {
    return await _ticketRepository.GetAllTickets();
}
```

**Benefits**:
1. **Non-Blocking**: Server can handle other requests while waiting for database
2. **Scalability**: More concurrent users with same resources
3. **Responsiveness**: UI doesn't freeze during long operations
4. **Best Practice**: Required for modern ASP.NET Core applications

### 7. Configuration Management

**Environment Variables via DotNetEnv**:

```csharp
// Program.cs:7
DotNetEnv.Env.TraversePath().Load();
```

**Configuration Sources**:
1. **.env file** (not checked into git): `Mongo:ConnectionString`
2. **appsettings.json**: `Mongo:Database`

**Why This Approach?**:
- **Security**: Connection strings with credentials not in source control
- **Flexibility**: Different connections for dev/staging/production
- **Best Practice**: Follows 12-factor app principles

**Example .env file**:
```
Mongo:ConnectionString=mongodb://localhost:27017
```

---

## Best Practices Applied

### 1. **Separation of Concerns**
- Controllers handle HTTP, not business logic
- Services contain business logic, not data access
- Repositories handle database operations only

### 2. **Dependency Injection**
- Interfaces define contracts
- Implementations injected at runtime
- Easy to swap implementations (e.g., different database)

### 3. **Interface-Driven Development**
- All services and repositories have interfaces
- Enables mocking for unit tests
- Enforces contracts between layers

### 4. **Logging**
- `ILogger<T>` injected into all controllers
- Structured logging with context
- Different log levels (Info, Warning, Error)

### 5. **Error Handling**
- Specific exception types caught first
- Generic exceptions caught as fallback
- User-friendly error messages
- All errors logged with details

### 6. **Validation**
- Client-side validation (Razor forms)
- Server-side validation (ModelState)
- Business logic validation (service layer)
- Data integrity validation (MongoDB)

### 7. **Security**
- Anti-forgery tokens on all POST requests
- Input validation on all user inputs
- ObjectId format validation
- No sensitive data in logs

### 8. **Performance**
- MongoDB connection pooling (via singleton MongoClient)
- Aggregation pipelines for complex queries
- Async/await for non-blocking I/O
- Field projection to minimize data transfer

### 9. **Code Organization**
- Consistent naming conventions
- Logical folder structure
- Clear separation of models, views, and controllers
- Reusable components (view models, shared views)

### 10. **Documentation**
- XML comments on public methods
- Clear variable names
- Meaningful commit messages
- This comprehensive documentation!

---

## Setup and Configuration

### Prerequisites

1. **.NET 9.0 SDK** installed
2. **MongoDB** server running (local or remote)
3. **IDE**: Visual Studio 2022, VS Code, or Rider

### Installation Steps

#### 1. Clone the Repository
```bash
git clone <repository-url>
cd NoSQL_Project
```

#### 2. Install Dependencies
```bash
cd NoSQL_Project
dotnet restore
```

#### 3. Configure MongoDB Connection

Create a `.env` file in the project root:
```env
Mongo:ConnectionString=mongodb://localhost:27017
```

Or update `appsettings.json`:
```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "IncidentManagement"
  }
}
```

#### 4. Run the Application
```bash
dotnet run
```

#### 5. Access the Application
Open browser to: `https://localhost:5001` or `http://localhost:5000`

### Database Setup

MongoDB collections will be created automatically on first access. No manual schema setup required!

**Optional: Seed Data**

You can manually add sample data via MongoDB shell:

```javascript
// Connect to database
use IncidentManagement;

// Add sample employee
db.Employee.insertOne({
  EmployeeId: 1001,
  Name: { FirstName: "John", LastName: "Doe" },
  Role: "ServiceDesk",
  contactInfo: {
    Email: "john.doe@company.com",
    Phone: "+1-555-0123",
    Location: "Building A, Floor 2"
  }
});

// Add sample ticket
db.Ticket.insertOne({
  TicketId: "INC-001",
  Title: "Computer won't start",
  Type: "Hardware",
  Priority: "High",
  Status: "Open",
  Deadline: "2025-10-15T17:00:00Z",
  Description: "Desktop computer shows black screen on startup",
  ReportedBy: ObjectId("<employee-id-from-above>"),
  HandeledBy: []
});
```

### Environment Configuration

**Development**:
- Uses `appsettings.Development.json`
- Detailed logging enabled
- Developer exception pages

**Production**:
- Uses `appsettings.json`
- HTTPS enforced
- HSTS enabled
- Error handling middleware

### Troubleshooting

**Problem**: "Mongo:ConnectionString is not configured"
**Solution**: Create `.env` file with connection string

**Problem**: "Unable to connect to MongoDB"
**Solution**: Ensure MongoDB service is running

**Problem**: "Database not found"
**Solution**: MongoDB will auto-create the database on first write operation

---

## Future Enhancements

### Potential Features

1. **Authentication & Authorization**
   - User login system
   - Role-based access control (Admin, ServiceDesk, User)
   - JWT tokens for API access

2. **Advanced Search & Filtering**
   - Full-text search on ticket descriptions
   - Filter by date range, priority, status
   - Sort by multiple columns

3. **Notifications**
   - Email notifications on ticket assignment
   - Deadline reminder emails
   - Real-time updates via SignalR

4. **Reporting**
   - Export tickets to CSV/Excel
   - Generate PDF reports
   - Custom date range reports

5. **Audit Trail**
   - Track all changes to tickets
   - Record who modified what and when
   - Compliance logging

6. **SLA Management**
   - Define service level agreements
   - Track SLA compliance
   - Escalation rules

7. **File Attachments**
   - Upload screenshots with tickets
   - Store in MongoDB GridFS or cloud storage
   - Preview attachments in browser

8. **API Layer**
   - RESTful API for external integrations
   - Swagger/OpenAPI documentation
   - API rate limiting

---

## Conclusion

This Incident Management System demonstrates a robust, scalable architecture built with modern best practices. The separation of concerns through the Repository-Service-Controller pattern ensures maintainability, while MongoDB's flexibility allows for rapid iteration and schema evolution.

**Key Achievements**:
- ✅ Full CRUD operations for Tickets and Employees
- ✅ Real-time dashboard analytics
- ✅ Proper error handling and logging
- ✅ Clean architecture with dependency injection
- ✅ Efficient MongoDB aggregation pipelines
- ✅ Comprehensive validation at all layers
- ✅ Responsive UI with Bootstrap
- ✅ Production-ready security measures

The implementation of complete CRUD operations was essential for providing a functional incident management system that supports the entire lifecycle of tickets and employee records, from creation through updates to eventual deletion, all while maintaining data integrity and providing excellent user experience.

---

**Document Version**: 1.0
**Last Updated**: October 2025
**Project**: NoSQL Incident Management System
**Framework**: ASP.NET Core 9.0 + MongoDB
