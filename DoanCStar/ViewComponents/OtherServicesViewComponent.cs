using Microsoft.AspNetCore.Mvc;

namespace DoanCStar.ViewComponents
{
    public class OtherServicesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}