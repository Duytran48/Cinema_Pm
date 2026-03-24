using Microsoft.AspNetCore.Mvc;
using DoanCStar.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using DoanCStar.Models;

namespace DoanCStar.ViewComponents
{
    public class ComingSoonViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ComingSoonViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // ===== THAY THẾ CHUỖI NÀY BẰNG STATUS CỦA BẠN =====
            const string statusSapChieu = "Sắp chiếu";
            // =================================================

            var movies = await _context.Movies
                                .Where(m => m.Status == statusSapChieu)
                                .OrderBy(m => m.MovieId) // Sắp xếp (ví dụ: cũ nhất sắp ra)
                                .Take(12)
                                .ToListAsync();

            return View(movies);
        }
    }
}