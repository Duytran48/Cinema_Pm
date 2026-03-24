using DoanCStar.Data;
using DoanCStar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoanCStar.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SearchController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Suggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new { movies = new List<object>(), cinemas = new List<object>() });

            var query = q.Trim().ToLower();

            // Tìm phim (chỉ Đang chiếu & Sắp chiếu)
            var movies = await _context.Movies
                .Where(m => (m.Status == "Đang chiếu" || m.Status == "Sắp chiếu")
                         && m.Title.ToLower().Contains(query))
                .OrderBy(m => m.Title)
                .Take(6)
                .Select(m => new
                {
                    type = "movie",
                    id = m.MovieId,
                    title = m.Title,
                    url = Url.Action("Details", "Movie", new { id = m.MovieId }),
                    image = m.ImageUrl ?? "/images/no-image.jpg"
                })
                .ToListAsync();

            // Tìm rạp
            var cinemas = await _context.Cinemas
                .Where(c => c.Name.ToLower().Contains(query) || c.City.ToLower().Contains(query))
                .OrderBy(c => c.City).ThenBy(c => c.Name)
                .Take(6)
                .Select(c => new
                {
                    type = "cinema",
                    id = c.CinemaId,
                    name = c.Name,
                    city = c.City,
                    url = Url.Action("Schedule", "Cinema", new { cinemaId = c.CinemaId })
                })
                .ToListAsync();

            return Json(new { movies, cinemas });
        }
    }
}