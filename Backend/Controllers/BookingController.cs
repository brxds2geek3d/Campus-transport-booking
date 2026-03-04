using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController(IConfiguration configuration) : ControllerBase
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        private readonly string _connectionString = configuration.GetConnectionString("AccessDb");
#pragma warning restore CS8601 // Possible null reference assignment.

        // --- PASTE THE CODE BELOW HERE ---
        [HttpPost]
        public IActionResult CreateBooking([FromBody] Booking b)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connectionString))
                {
                    conn.Open();
                    // In Access (OleDb), we use '?' as placeholders. 
                    // They don't have names, so the order you add parameters below MUST match this list.
                    string sql = "INSERT INTO Bookings (UserID, ScheduleID, SeatNumber, BookingStatus, BookingTime, Price) " +
                                 "VALUES (?, ?, ?, ?, ?, ?)";

                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        // 1. UserID (Match 'Number' in Access)
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = b.UserID;
                        
                        // 2. ScheduleID (Match 'Number' in Access)
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = b.ScheduleID;
                        
                        // 3. SeatNumber (Match 'Short Text' in Access)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = (object)b.SeatNumber ?? "";
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                        
                        // 4. BookingStatus (Match 'Short Text' in Access)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = (object)b.BookingStatus ?? "Confirmed";
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                        
                        // 5. BookingTime (Match 'Date/Time' in Access)
                        cmd.Parameters.Add("?", OleDbType.Date).Value = DateTime.Now;
                        
                        // 6. Price (Match 'Currency' in Access)
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = b.Price;

                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok(new { message = "Successfully Booked!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserBookings(int userId)
        {
            var myBookings = new List<object>();
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connectionString))
                {
                    conn.Open();
                    // This query joins 4 tables to give the user readable information
                    string sql = @"SELECT B.BookingID, B.BookingStatus, S.[Departure Time], 
                                          R.StartPoint, R.Destination, Bus.PlateNumber
                                   FROM (((Bookings B
                                   INNER JOIN Schedules S ON B.ScheduleID = S.ScheduleID)
                                   INNER JOIN Routes R ON S.RouteID = R.RouteID)
                                   INNER JOIN Buses Bus ON S.BusID = Bus.BusID)
                                   WHERE B.UserID = ?";

                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("?", userId);
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                myBookings.Add(new {
                                    BookingID = reader["BookingID"],
                                    Route = reader["StartPoint"] + " to " + reader["Destination"],
                                    Plate = reader["PlateNumber"],
                                    Time = reader["Departure Time"]?.ToString(),
                                    Status = reader["BookingStatus"]
                                });
                            }
                        }
                    }
                }
                return Ok(myBookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("admin/all")]
public IActionResult GetAllBookings()
{
    var allBookings = new List<object>();
    try
    {
        using (OleDbConnection conn = new OleDbConnection(_connectionString))
        {
            conn.Open();
            // We use [] for names with spaces or special characters like (km)
            // Double-check if your Users table uses 'FullName' or just 'Name'
            string sql = @"SELECT B.BookingID, U.[Full Name], R.StartPoint, R.Destination, 
                                  Bus.PlateNumber, S.[Departure Time]
                           FROM ((((Bookings AS B
                           INNER JOIN Users AS U ON B.UserID = U.UserID)
                           INNER JOIN Schedules AS S ON B.ScheduleID = S.ScheduleID)
                           INNER JOIN Routes AS R ON S.RouteID = R.RouteID)
                           INNER JOIN Buses AS Bus ON S.BusID = Bus.BusID)";

            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    allBookings.Add(new {
                        BookingID = reader["BookingID"],
                        UserName = reader["Full Name"], 
                        Route = reader["StartPoint"].ToString() + " to " + reader["Destination"].ToString(),
                        Plate = reader["PlateNumber"],
                        Time = reader["Departure Time"]?.ToString()
                    });
                }
            }
        }
        return Ok(allBookings);
    }
    catch (Exception ex)
    {
        // This will now print the exact SQL error to your console
        return BadRequest(new { error = ex.Message });
    }
}
    }
}