namespace BookingService.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string VehicleId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
