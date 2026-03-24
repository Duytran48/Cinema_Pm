using System.ComponentModel.DataAnnotations.Schema;

namespace DoanCStar.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int ShowTimeId { get; set; }
        public string BookingCode { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int? PromotionId { get; set; }

        public User User { get; set; }
        public ShowTime ShowTime { get; set; }
        public Promotion Promotion { get; set; }

        public Payment Payment { get; set; }

        public ICollection<BookingSeat> BookingSeats { get; set; }
        public ICollection<BookingConcession> BookingConcessions { get; set; }
        [NotMapped]
        public List<int> SelectedSeatIds { get; set; }

    }
}
