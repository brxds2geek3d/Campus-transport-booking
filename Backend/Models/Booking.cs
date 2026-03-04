namespace Backend.Models
{
    public class Booking
    {
        public int UserID { get; set; }
        public int ScheduleID { get; set; }
        public string? SeatNumber { get; set; }
        public string? BookingStatus { get; set; }
        public DateTime BookingTime { get; set; }
        public decimal Price { get; set; }
    }
}