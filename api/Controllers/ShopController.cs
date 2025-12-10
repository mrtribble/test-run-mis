using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using api.Models;
using api.Services;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopController : ControllerBase
{
    private readonly DatabaseService _dbService;
    private readonly ILogger<ShopController> _logger;

    public ShopController(DatabaseService dbService, ILogger<ShopController> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }

    // GET: api/shop
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Shop>>> GetShops()
    {
        try
        {
            using var connection = await _dbService.GetConnectionAsync();
            var shops = new List<Shop>();

            var query = @"
                SELECT ShopID, ShopName, Rating, DateEntered, Favorited, Deleted
                FROM shop
                WHERE Deleted = 0
                ORDER BY Rating DESC";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                shops.Add(new Shop
                {
                    ShopID = reader.GetInt32(reader.GetOrdinal("ShopID")),
                    ShopName = reader.GetString(reader.GetOrdinal("ShopName")),
                    Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                    DateEntered = reader.GetDateTime(reader.GetOrdinal("DateEntered")),
                    Favorited = reader.GetBoolean(reader.GetOrdinal("Favorited")),
                    Deleted = reader.GetBoolean(reader.GetOrdinal("Deleted"))
                });
            }

            return Ok(shops);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shops: {Message}", ex.Message);
            var errorMessage = "Error retrieving shops";
            if (ex.Message.Contains("connection string") || ex.Message.Contains("not configured"))
            {
                errorMessage = "Database connection not configured. Please set DATABASE_URL or configure DefaultConnection in appsettings.json";
            }
            else if (ex.Message.Contains("Table") && ex.Message.Contains("doesn't exist"))
            {
                errorMessage = "Database table 'shop' does not exist. Please check database initialization.";
            }
            else
            {
                errorMessage = $"Error retrieving shops: {ex.Message}";
            }
            return StatusCode(500, new { message = errorMessage, details = ex.Message });
        }
    }

    // POST: api/shop
    [HttpPost]
    public async Task<ActionResult<Shop>> CreateShop([FromBody] Shop shop)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(shop.ShopName))
            {
                return BadRequest(new { message = "Shop name is required" });
            }

            if (shop.Rating < 0 || shop.Rating > 5)
            {
                return BadRequest(new { message = "Rating must be between 0 and 5" });
            }

            using var connection = await _dbService.GetConnectionAsync();

            var query = @"
                INSERT INTO shop (ShopName, Rating, DateEntered, Favorited, Deleted)
                VALUES (@ShopName, @Rating, @DateEntered, @Favorited, @Deleted);
                SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ShopName", shop.ShopName);
            command.Parameters.AddWithValue("@Rating", shop.Rating);
            command.Parameters.AddWithValue("@DateEntered", DateTime.Now);
            command.Parameters.AddWithValue("@Favorited", false);
            command.Parameters.AddWithValue("@Deleted", false);

            var shopId = Convert.ToInt32(await command.ExecuteScalarAsync());

            shop.ShopID = shopId;
            shop.DateEntered = DateTime.Now;
            shop.Favorited = false;
            shop.Deleted = false;

            return CreatedAtAction(nameof(GetShops), new { id = shopId }, shop);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shop");
            return StatusCode(500, new { message = "Error creating shop" });
        }
    }

    // PUT: api/shop/{id}/favorite
    [HttpPut("{id}/favorite")]
    public async Task<ActionResult> ToggleFavorite(int id)
    {
        try
        {
            using var connection = await _dbService.GetConnectionAsync();

            // First get current favorite status
            var getQuery = "SELECT Favorited FROM shop WHERE ShopID = @ShopID AND Deleted = 0";
            using var getCommand = new MySqlCommand(getQuery, connection);
            getCommand.Parameters.AddWithValue("@ShopID", id);

            var currentFavorite = await getCommand.ExecuteScalarAsync();
            if (currentFavorite == null)
            {
                return NotFound(new { message = "Shop not found" });
            }

            bool newFavoriteStatus = !Convert.ToBoolean(currentFavorite);

            // Update favorite status
            var updateQuery = "UPDATE shop SET Favorited = @Favorited WHERE ShopID = @ShopID";
            using var updateCommand = new MySqlCommand(updateQuery, connection);
            updateCommand.Parameters.AddWithValue("@Favorited", newFavoriteStatus);
            updateCommand.Parameters.AddWithValue("@ShopID", id);

            await updateCommand.ExecuteNonQueryAsync();

            return Ok(new { favorited = newFavoriteStatus });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite");
            return StatusCode(500, new { message = "Error updating favorite status" });
        }
    }

    // DELETE: api/shop/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteShop(int id)
    {
        try
        {
            using var connection = await _dbService.GetConnectionAsync();

            var query = "UPDATE shop SET Deleted = 1 WHERE ShopID = @ShopID";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ShopID", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound(new { message = "Shop not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shop");
            return StatusCode(500, new { message = "Error deleting shop" });
        }
    }
}

