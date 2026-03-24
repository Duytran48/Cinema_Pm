using DoanCStar.Data;
using DoanCStar.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoanCStar.ViewComponents
{
    public class CinemaListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public CinemaListViewComponent(ApplicationDbContext context) => _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Số lượng dễ chỉnh
            const int MaxCities = 4;
            const int MaxCinemasPerCity = 5;

            var cinemas = await _context.Cinemas
                .OrderBy(c => c.City)
                .ThenBy(c => c.Name)
                .Select(c => new CinemaDropdownViewModel
                {
                    CinemaId = c.CinemaId,
                    Name = c.Name,
                    City = c.City ?? "Khác"
                })
                .ToListAsync();

            var grouped = cinemas
                .GroupBy(c => c.City)
                .Take(MaxCities) // tối đa 4 thành phố
                .ToDictionary(
                    g => g.Key,
                    g => g.Take(MaxCinemasPerCity).ToList() // tối đa 5 rạp mỗi thành phố
                );

            return View(grouped);
        }
    }
}