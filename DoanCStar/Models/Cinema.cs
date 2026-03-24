namespace DoanCStar.Models
{
    public class Cinema
    {
        public int CinemaId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }

        public ICollection<Room> Rooms { get; set; }
    }
}
