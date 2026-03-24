namespace DoanCStar.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string Director { get; set; }
        public string Genre { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string TrailerUrl { get; set; }
        public string Language { get; set; }
        public bool Subtitle { get; set; }
        public bool Dubbed { get; set; }
        public string Status { get; set; }

        public ICollection<ShowTime> ShowTimes { get; set; }
    }
}
