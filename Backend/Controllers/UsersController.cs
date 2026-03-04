using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly string _connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=""C:\Users\admin\OneDrive\Documents\UON\CampusTransportSystem\Backend\Database\CampusTransport.accdb"";Persist Security Info=False;";

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = new List<object>();
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connString))
                {
                    string sql = "SELECT UserID, [Full Name], [Role] FROM Users";
                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new {
                                userID = reader["UserID"],
                                fullName = reader["Full Name"].ToString(),
                                role = reader["Role"].ToString()
                            });
                        }
                    }
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

       [HttpGet("login")]
public IActionResult Login(string email, string password)
{
    try
    {
        using (OleDbConnection conn = new OleDbConnection(_connString))
        {
            conn.Open();
            // We select the ID and Name so the frontend can use them for tickets
            string sql = "SELECT UserID, [Full Name], [Role] FROM Users WHERE Email = ? AND [Password] = ?";
            
            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("?", email);
                cmd.Parameters.AddWithValue("?", password);

                using (OleDbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // This object is what gets saved into localStorage
                        return Ok(new {
                            userID = reader["UserID"],
                            fullName = reader["Full Name"].ToString(),
                            role = reader["Role"].ToString()
                        });
                    }
                }
            }
        }
        return Unauthorized(new { error = "Invalid email or password." });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
    public class UserModel {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
}