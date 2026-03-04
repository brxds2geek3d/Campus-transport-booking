using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        // Absolute path to your database
        private readonly string _connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=""C:\Users\admin\OneDrive\Documents\UON\CampusTransportSystem\Backend\Database\CampusTransport.accdb"";Persist Security Info=False;";

        [HttpGet("available-schedules")]
        public IActionResult GetAvailableSchedules()
        {
            var schedules = new List<object>();
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connString))
                {
                    string sql = @"
                        SELECT 
                            S.ScheduleID,
                            R.StartPoint,
                            R.Destination,
                            R.[Distance(km)],
                            S.[Departure Time],
                            S.[Arrival Time],
                            B.Capacity,
                            B.PlateNumber,
                            R.Price,
                            (SELECT COUNT(*) FROM Bookings WHERE ScheduleID = S.ScheduleID) AS BookedCount
                        FROM (Schedules AS S
                        INNER JOIN Routes AS R ON S.RouteID = R.RouteID)
                        INNER JOIN Buses AS B ON S.BusID = B.BusID";

                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            schedules.Add(new {
                                ScheduleID = reader["ScheduleID"],
                                From = reader["StartPoint"].ToString(),
                                To = reader["Destination"].ToString(),
                                Distance = reader["Distance(km)"].ToString() + " km",
                                DepartureTime = Convert.ToDateTime(reader["Departure Time"]).ToString("hh:mm tt"),
                                ArrivalTime = Convert.ToDateTime(reader["Arrival Time"]).ToString("hh:mm tt"),
                                AvailableSeats = Convert.ToInt32(reader["Capacity"]) - Convert.ToInt32(reader["BookedCount"]),
                                Plate = reader["PlateNumber"],
                                Price = reader["Price"]
                            });
                        }
                    }
                }
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpPost("book")]
public IActionResult BookTicket([FromBody] BookingRequest req)
{
    try {
        using (OleDbConnection conn = new OleDbConnection(_connString)) {
            conn.Open();
            // Use ? for parameters in Access
            string sql = "INSERT INTO Bookings (UserID, ScheduleID, BookingDate) VALUES (?, ?, ?)";
            using (OleDbCommand cmd = new OleDbCommand(sql, conn)) {
                cmd.Parameters.AddWithValue("?", req.UserID);
                cmd.Parameters.AddWithValue("?", req.ScheduleID);
                cmd.Parameters.AddWithValue("?", DateTime.Now.ToString());
                cmd.ExecuteNonQuery();
            }
        }
        return Ok(new { message = "Database updated!" });
    } catch (Exception ex) {
        return BadRequest(new { error = ex.Message });
    }
}

public class BookingRequest {
    public int UserID { get; set; }
    public int ScheduleID { get; set; }
}
    }
}