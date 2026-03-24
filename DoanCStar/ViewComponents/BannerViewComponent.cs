using Microsoft.AspNetCore.Mvc;
namespace DoanCStar.ViewComponents
{
    public class BannerViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
