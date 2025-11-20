# M-Suite Enterprise Application

## Overview
M-Suite is a robust enterprise web application designed to streamline business operations for organizations of all sizes. It covers sales, warehousing, inventory management, user authentication, geo-location tracking, and advanced AI-powered analytics. Built with ASP.NET Core MVC and Entity Framework Core, M-Suite is scalable, secure, and easy to extend.

## Features
- **User Authentication:** Secure login system using BCrypt password hashing and cookie-based authentication
- **Warehouse & Inventory Management:** Track products, stock levels, and transactions efficiently
- **Sales & Order Processing:** Manage orders, customers, and sales activities with full workflow support
- **Geo-Location Tracking:** Monitor deliveries and field operations in real time
- **Reporting & Analytics:** Generate actionable business insights with comprehensive dashboards
- **AI Capabilities:**
  - Chatbot assistant for user support and navigation
  - Item correlation analysis for cross-selling and recommendations
  - AI dashboard for insights and anomaly detection
- **Security:** Role-based access control, password hashing, session management
- **Modern UI:** Responsive design with Bootstrap, intuitive navigation, and real-time updates

## Tech Stack
- **Backend:** ASP.NET Core MVC 9.0
- **Database:** Microsoft SQL Server (Entity Framework Core, Database-First)
- **Authentication:** Cookie-based authentication with BCrypt password hashing
- **Frontend:** HTML, CSS, JavaScript, Bootstrap 5, jQuery
- **AI/ML:** ML.NET, Matrix Factorization
- **Security:** BCrypt.Net-Next for password hashing

## Prerequisites
- .NET 9.0 SDK or later
- Microsoft SQL Server (Express, Standard, or Enterprise)
- Visual Studio 2022 or VS Code (recommended)
- SQL Server Management Studio (SSMS) for database management

## Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/MALIA/M-Suite.git
cd M-Suite/M-Suite
```

### 2. Configure the Database Connection

Edit `appsettings.json` and update the connection string to match your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "MSuiteContext": "Server=YOUR_SERVER_NAME\\SQLEXPRESS;Database=MSuite_Malia_Sap;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Important:** Replace `YOUR_SERVER_NAME` with your actual SQL Server instance name. Common examples:
- `localhost\\SQLEXPRESS` (SQL Server Express)
- `localhost` (Default instance)
- `SERVER_NAME\\INSTANCE_NAME` (Named instance)

### 3. Database Setup

The application uses Database-First approach. Ensure your database exists and is accessible:

1. **Option A: Use Existing Database**
   - Make sure the database `MSuite_Malia_Sap` exists
   - Verify the connection string matches your server

2. **Option B: Create New Database** (if needed)
   ```sql
   CREATE DATABASE MSuite_Malia_Sap;
   ```

### 4. Install Dependencies
```bash
dotnet restore
```

This will install all NuGet packages including:
- Microsoft.EntityFrameworkCore.SqlServer
- BCrypt.Net-Next
- Microsoft.ML
- DinkToPdf
- And other required packages

### 5. Build the Application
```bash
dotnet build
```

### 6. Run the Application

**Option 1: Using dotnet CLI**
```bash
dotnet run
```

**Option 2: Using Visual Studio**
- Open `MALIA.sln` in Visual Studio
- Press F5 or click "Run"

**Option 3: Using VS Code**
- Open the project folder in VS Code
- Press F5 or use the terminal: `dotnet run`

### 7. Access the Application

Once running, the application will be available at:
- **HTTP:** `http://localhost:5000` (or the port shown in the console)
- **HTTPS:** `https://localhost:5001` (if configured)

The default route redirects to the login page: `/User/Login`

## First-Time Setup

### Creating Your First User

1. **If you have database access:**
   - Create a user directly in the database with a plain text password
   - On first login, the password will be automatically hashed using BCrypt

2. **If you need to create a user via the application:**
   - You'll need admin access or database access to create the first user
   - Passwords are automatically hashed when creating/editing users

### Default Login
- Check your database for existing users
- Passwords are stored as BCrypt hashes (starting with `$2a$`, `$2b$`, or `$2y$`)
- Plain text passwords are automatically converted to hashes on first login

## Application Structure

```
M-Suite/
├── Controllers/          # MVC Controllers
│   ├── UserController.cs
│   ├── ItemController.cs
│   ├── TransactionController.cs
│   └── ...
├── Models/              # Entity Framework Models (Database-First)
├── Views/               # Razor Views
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ChatbotPartial.cshtml
│   └── ...
├── Services/            # Business Logic Services
│   ├── TransactionService.cs
│   ├── ChatbotService.cs
│   └── ItemCorrelationService.cs
├── Context/             # Database Context
│   └── MaliaContext.cs
├── wwwroot/            # Static Files (CSS, JS, Images)
└── Program.cs          # Application Entry Point
```

## Key Features & Improvements

### Security Enhancements
- ✅ BCrypt password hashing for all user passwords
- ✅ Cookie-based authentication with secure session management
- ✅ Role-based authorization on all controllers
- ✅ User active status and expiry date validation
- ✅ Soft delete support for users

### Functionality Improvements
- ✅ Search and pagination on Item and User lists
- ✅ Proper form validation with error messages
- ✅ Transaction number generation with race condition protection
- ✅ Comprehensive error handling and null checks
- ✅ Improved dropdown lists with proper display values
- ✅ Success/error message display throughout the application

### UI/UX Improvements
- ✅ Modern sidebar navigation with all menu items
- ✅ Responsive design for mobile devices
- ✅ Real-time clock and user profile dropdown
- ✅ Alert messages for success/error feedback
- ✅ Improved dashboard with null-safe queries

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check the connection string in `appsettings.json`
- Ensure the database exists
- Verify Windows Authentication or SQL Authentication credentials

### Port Already in Use
- Change the port in `Properties/launchSettings.json`
- Or stop the process using the port

### Build Errors
- Run `dotnet clean` then `dotnet restore`
- Ensure .NET 9.0 SDK is installed
- Check that all NuGet packages are restored

### Login Issues
- Verify user exists in database
- Check that user is active (`UsActive = 1`)
- Verify user hasn't expired (`UsExpiryDate`)
- Ensure user is not deleted (`UsDeleted = 0`)

## Development

### Running in Development Mode
The application uses `appsettings.Development.json` for development-specific settings. Make sure this file exists and has the correct connection string.

### Adding New Features
1. Create models in the `Models/` directory
2. Add controllers in `Controllers/`
3. Create views in `Views/`
4. Add services in `Services/` for business logic
5. Update the database context if needed

## Production Deployment

### Important Considerations
1. **Change Connection String:** Use a secure connection string for production
2. **Enable HTTPS:** Configure SSL certificates
3. **Update Session Timeout:** Adjust in `Program.cs`
4. **Enable Logging:** Configure proper logging for production
5. **Database Backups:** Set up regular database backups
6. **Security:** Review and harden security settings

## API Endpoints

The application includes API endpoints for:
- Item management: `/api/Item`
- AI Chatbot: `/api/AI/chatbot`
- Item Correlations: `/api/AI/correlations/{itemId}`

All API endpoints require authentication.

## Support

For issues, questions, or contributions:
- Check the codebase documentation
- Review the in-app help (Chatbot)
- Contact the development team

## License
This project is proprietary to Malia Groups. For licensing inquiries, please contact the maintainers.

---

## Quick Start Summary

```bash
# 1. Navigate to project
cd M-Suite/M-Suite

# 2. Update connection string in appsettings.json

# 3. Restore packages
dotnet restore

# 4. Build
dotnet build

# 5. Run
dotnet run

# 6. Open browser to http://localhost:5000
```

**That's it!** The application should now be running and ready to use.



