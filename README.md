# M-Suite Enterprise Application

## Overview
M-Suite is a robust enterprise web application designed to streamline business operations for organizations of all sizes. It covers sales, warehousing, inventory management, user authentication, geo-location tracking, and advanced AI-powered analytics. Built with ASP.NET Core MVC and Entity Framework Core, M-Suite is scalable, secure, and easy to extend.

## Features
- **User Authentication:** Secure login system using cookie-based authentication.
- **Warehouse & Inventory Management:** Track products, stock levels, and transactions efficiently.
- **Sales & Order Processing:** Manage orders, customers, and sales activities.
- **Geo-Location Tracking:** Monitor deliveries and field operations in real time.
- **Reporting & Analytics:** Generate actionable business insights.
- **AI Capabilities:**
  - Chatbot assistant for user support and navigation
  - Item correlation analysis for cross-selling and recommendations
  - AI dashboard for insights and anomaly detection

## Tech Stack
- **Backend:** ASP.NET Core MVC
- **Database:** Microsoft SQL Server (Entity Framework Core, Database-First)
- **Authentication:** Cookie-based
- **Frontend:** HTML, CSS, JavaScript, Bootstrap
- **AI/ML:** ML.NET, Matrix Factorization
- **Version Control:** Git & GitHub

## Getting Started
### 1. Clone the Repository
```bash
git clone https://github.com/MALIA/M-Suite.git
cd M-Suite
```

### 2. Configure the Database Connection
Edit `M-Suite/appsettings.json` and update the connection string:
```json
"ConnectionStrings": {
  "MSuiteContext": "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Apply Migrations (If Needed)
```bash
dotnet ef database update
```

### 5. Run the Application
```bash
dotnet run
```

## Usage
- Access the web app at `http://localhost:5000` (or the port specified in your launch settings).
- Log in with your credentials to access features based on your role (admin, supervisor, etc.).
- Use the AI dashboard for analytics and the chatbot for assistance.

## AI Features
- **Chatbot Assistant:** Natural language interface for help and navigation.
- **Item Correlation Analysis:** Discover product relationships and optimize sales.
- **AI Dashboard:** Visualize insights, patterns, and anomalies.

## Contribution Guidelines
1. **Fork the Repository**
2. **Create a New Branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Commit Changes:**
   ```bash
   git commit -m "Describe your changes"
   ```
4. **Push to GitHub:**
   ```bash
   git push origin feature/your-feature-name
   ```
5. **Create a Pull Request**

## License
This project is proprietary to Malia Groups. For licensing inquiries, please contact the maintainers.

---
For more information, see the in-app documentation or contact the project maintainers.



