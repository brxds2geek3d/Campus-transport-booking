using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusesController(IConfiguration configuration) : ControllerBase
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        private readonly string _connectionString = configuration.GetConnectionString("AccessDb");
#pragma warning restore CS8601 // Possible null reference assignment.

        // 1. GET: api/buses (Used by Admin to see the fleet)
        [HttpGet]
        public IActionResult GetBuses()
        {
            var buses = new List<object>();
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connectionString))
                {
                    conn.Open();
                    // Status is bracketed as it can be a reserved keyword
                    string sql = "SELECT BusID, PlateNumber, Capacity, [Status] FROM Buses";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                buses.Add(new
                                {
                                    BusID = reader["BusID"],
                                    PlateNumber = reader["PlateNumber"],
                                    Capacity = reader["Capacity"],
                                    Status = reader["Status"]
                                });
                            }
                        }
                    }
                }
                return Ok(buses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // 2. POST: api/buses (Used by Admin to add a new bus)
        [HttpPost]
        public IActionResult AddBus([FromBody] BusModel newBus)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Buses (PlateNumber, Capacity, [Status]) VALUES (?, ?, ?)";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("?", newBus.PlateNumber);
                        cmd.Parameters.AddWithValue("?", newBus.Capacity);
                        cmd.Parameters.AddWithValue("?", newBus.Status ?? "Active"); // Default to Active
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok(new { message = "Bus registered successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // Simple class to map the incoming JSON data
    public class BusModel
    {
        public required string PlateNumber { get; set; }
        public int Capacity { get; set; }
        public required string Status { get; set; }
    }
}
