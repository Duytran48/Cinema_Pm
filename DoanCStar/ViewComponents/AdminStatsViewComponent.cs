using Microsoft.AspNetCore.Mvc;
using DoanCStar.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DoanCStar.ViewComponents
{
    public class AdminStatsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public AdminStatsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var totalMovies = await _context.Movies.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalPromotions = await _context.Promotions.CountAsync();

            var totalRevenue = await _context.Payments.SumAsync(p => p.Amount);

            var model = (
                TotalMovies: totalMovies,
                TotalUsers: totalUsers,
                TotalPromotions: totalPromotions,
                TotalRevenue: totalRevenue
            );

            return View(model); 
        }
    }
}