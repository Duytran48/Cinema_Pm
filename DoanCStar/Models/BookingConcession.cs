namespace DoanCStar.Models
{
    public class BookingConcession
    {
        public int BookingConcessionId { get; set; }
        public int BookingId { get; set; }
        public int ConcessionId { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }

        public Booking Booking { get; set; }
        public Concession Concession { get; set; }
    }
}
