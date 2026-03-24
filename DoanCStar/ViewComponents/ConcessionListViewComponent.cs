using Microsoft.AspNetCore.Mvc;
using DoanCStar.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace DoanCStar.ViewComponents
{
    public class ConcessionListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ConcessionListViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _context.Concessions.Where(c => c.Active).ToListAsync();
            return View(items);
        }
    }
}