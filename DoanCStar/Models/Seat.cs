using System.ComponentModel.DataAnnotations.Schema;

namespace DoanCStar.Models
{
    public class Seat
    {
        public int SeatId { get; set; }
        public int RoomId { get; set; }
        public string Row { get; set; }
        public int Number { get; set; }
        public string SeatType { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public Room Room { get; set; }
    }
}
