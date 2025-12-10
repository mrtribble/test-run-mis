# coffee.co - Coffee Shop Review Site

A full-stack web application for reviewing and managing coffee shops.

## Features

- ✅ View all coffee shops (sorted by rating, highest first)
- ✅ Add new coffee shops with name and rating
- ✅ Favorite/unfavorite coffee shops
- ✅ Delete coffee shops (soft delete)
- ✅ Persistent data storage in MySQL database

## Technology Stack

- **Frontend**: HTML, JavaScript (ES6+), Bootstrap 5.x
- **Backend**: .NET 8.0 Web API (C#)
- **Database**: MySQL (Heroku MySQL)

## Project Structure

```
test-run-mis/
├── api/                    # Backend .NET API
│   ├── Controllers/        # API controllers
│   ├── Models/            # Data models
│   ├── Services/          # Database service
│   └── Program.cs         # Application entry point
└── client/                # Frontend
    ├── index.html         # Main HTML file
    └── resources/
        ├── scripts/       # JavaScript files
        └── styles/        # CSS files
```

## Setup Instructions

### Prerequisites

- .NET 8.0 SDK
- MySQL database (Heroku MySQL or local MySQL)
- Web browser

### Backend Setup

1. Navigate to the `api` directory:
   ```bash
   cd api
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Configure database connection:
   - For Heroku: The `DATABASE_URL` environment variable will be automatically read
   - For local development: Update `appsettings.json` with your MySQL connection string:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=coffeeshop;User Id=youruser;Password=yourpassword;"
     }
     ```

4. Run the API:
   ```bash
   dotnet run
   ```

   The API will be available at:
   - HTTPS: `https://localhost:7000`
   - HTTP: `http://localhost:5000`
   - Swagger UI: `https://localhost:7000/swagger`

### Frontend Setup

1. Open `client/index.html` in a web browser, or

2. Use a local web server (recommended):
   ```bash
   # Using Python
   cd client
   python -m http.server 8000
   
   # Using Node.js http-server
   npx http-server client -p 8000
   ```

3. Update API URL in `client/resources/scripts/api.js` if needed:
   ```javascript
   const API_BASE_URL = 'https://localhost:7000/api';
   ```

### Database Schema

The `shop` table is automatically created on first run with the following structure:

- `ShopID` (INT, Primary Key, Auto Increment)
- `ShopName` (VARCHAR(255), Required)
- `Rating` (DECIMAL(3,2), Required, 0-5)
- `DateEntered` (DATETIME, Default: Current Timestamp)
- `Favorited` (BOOLEAN, Default: FALSE)
- `Deleted` (BOOLEAN, Default: FALSE) - for soft deletes

## API Endpoints

- `GET /api/shop` - Get all non-deleted shops (sorted by rating DESC)
- `POST /api/shop` - Create a new shop
  ```json
  {
    "shopName": "Coffee Shop Name",
    "rating": 4.5
  }
  ```
- `PUT /api/shop/{id}/favorite` - Toggle favorite status
- `DELETE /api/shop/{id}` - Soft delete a shop

## Usage

1. Start the backend API server
2. Open the frontend in a web browser
3. Add coffee shops using the form
4. Click the star (☆) to favorite a shop
5. Click "Delete" to remove a shop
6. Shops are automatically sorted by rating (highest first)

## Notes

- All shops are sorted by rating in descending order
- Deleted shops are hidden but not removed from the database (soft delete)
- The database table is automatically created on first API startup
- CORS is enabled for all origins (configure as needed for production)
