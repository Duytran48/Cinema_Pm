using DoanCStar.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DoanCStar.ViewComponents
{
    public class BookingSummaryViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaymentViewModel vm) => View(vm);
    }
}
