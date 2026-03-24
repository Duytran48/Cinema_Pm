namespace DoanCStar.Models
{
    public class ShowTime
    {
        public int ShowTimeId { get; set; }
        public int MovieId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }

        public Movie Movie { get; set; }
        public Room Room { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
