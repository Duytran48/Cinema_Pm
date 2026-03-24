using DoanCStar.Models;
using System.Collections.Generic;

namespace DoanCStar.ViewModels
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }
        public List<ShowTime> ShowTimes { get; set; }
    }
}