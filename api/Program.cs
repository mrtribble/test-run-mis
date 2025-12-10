using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddCors(options => 
{ 
    options.AddPolicy("OpenPolicy", builder => 
    { 
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader(); 
    }); 
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure MySQL connection
// Heroku provides DATABASE_URL environment variable
// Format: mysql://user:password@host:port/database
string? connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback to appsettings.json for local development
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// Parse Heroku DATABASE_URL format if present
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("mysql://"))
{
    try
    {
        var uri = new Uri(connectionString);
        var db = uri.AbsolutePath.TrimStart('/');
        var user = uri.UserInfo.Split(':')[0];
        var passwd = uri.UserInfo.Split(':')[1];
        // Default to port 3306 if not specified (uri.Port returns -1 if not in URL)
        var port = uri.Port == -1 ? 3306 : uri.Port;
        connectionString = $"Server={uri.Host};Port={port};Database={db};User Id={user};Password={passwd};SSL Mode=Required;";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
        Console.WriteLine("Please ensure the DATABASE_URL is complete and in the format: mysql://user:password@host:port/database");
        connectionString = null; // Reset to null so user knows it's invalid
    }
}

// Register database service - always register, even if connection string is empty
// This prevents DI errors, but database operations will fail if connection string is missing
builder.Services.AddSingleton<api.Services.DatabaseService>(provider => 
    new api.Services.DatabaseService(connectionString ?? ""));

// Initialize database table on startup if connection string is available
if (!string.IsNullOrEmpty(connectionString))
{
    InitializeDatabase(connectionString);
}
else
{
    Console.WriteLine("WARNING: No database connection string found!");
    Console.WriteLine("Please set DATABASE_URL environment variable or configure DefaultConnection in appsettings.json");
    Console.WriteLine("The API will start but database operations will fail.");
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("OpenPolicy");
app.MapControllers();

app.Run();

// Initialize database table if it doesn't exist
void InitializeDatabase(string connectionString)
{
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        
        var createTableQuery = @"
            CREATE TABLE IF NOT EXISTS shop (
                ShopID INT AUTO_INCREMENT PRIMARY KEY,
                ShopName VARCHAR(255) NOT NULL,
                Rating DECIMAL(3,2) NOT NULL,
                DateEntered DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                Favorited BOOLEAN NOT NULL DEFAULT FALSE,
                Deleted BOOLEAN NOT NULL DEFAULT FALSE
            )";
        
        using var command = new MySqlCommand(createTableQuery, connection);
        command.ExecuteNonQuery();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
    }
}

