using Microsoft.AspNetCore.Mvc;

namespace DoanCStar.ViewComponents
{
    public class BookingStepsViewComponent : ViewComponent
    {
        // step = 1, 2, 3
        public IViewComponentResult Invoke(int step)
        {
            return View("Default",step); // dùng view "Default"
        }
    }
}
