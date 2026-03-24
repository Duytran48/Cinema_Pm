namespace DoanCStar.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public int CinemaId { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }

        public Cinema Cinema { get; set; }
        public ICollection<Seat> Seats { get; set; }
        public ICollection<ShowTime> ShowTimes { get; set; }

    }
}
