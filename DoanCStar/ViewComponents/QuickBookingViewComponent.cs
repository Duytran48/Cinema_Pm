using DoanCStar.Data;
using DoanCStar.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoanCStar.ViewComponents
{
    public class QuickBookingViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public QuickBookingViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        // Load ban đầu: Chỉ load danh sách rạp (tuần tự bắt đầu từ rạp như Cinestar)
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cinemas = await _context.Cinemas
                .OrderBy(c => c.City)
                .ThenBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.CinemaId.ToString(),
                    Text = $"{c.Name} - {c.City}"
                })
                .ToListAsync();

            var model = new QuickBookingViewModel
            {
                Cinemas = cinemas
            };

            return View(model);
        }
    }
}