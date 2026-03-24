using Microsoft.AspNetCore.Mvc;
using DoanCStar.Data;
using Microsoft.EntityFrameworkCore; 
using System.Threading.Tasks; 
using System.Linq; 
using DoanCStar.Models;

namespace DoanCStar.ViewComponents
{
    public class NowShowingViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;


        public NowShowingViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string status = "Đang chiếu", int count = 12)
        {
 
            const string statusDangChieu = "Đang chiếu";
  

            var movies = await _context.Movies
                                .Where(m => m.Status == statusDangChieu)
                                .OrderByDescending(m => m.MovieId) 
                                .Take(count) 
                                .ToListAsync();
            ViewBag.SectionTitle = status == "Đang chiếu" ? "PHIM ĐANG CHIẾU" : "PHIM SẮP CHIẾU";
            ViewBag.StatusValue = status;
            return View(movies);
        }
    }
}