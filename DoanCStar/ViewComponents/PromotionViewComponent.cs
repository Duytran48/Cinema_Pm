using Microsoft.AspNetCore.Mvc;

namespace DoanCStar.ViewComponents
{
    public class PromotionViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}