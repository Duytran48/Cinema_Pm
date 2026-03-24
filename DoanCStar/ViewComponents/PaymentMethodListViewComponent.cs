using DoanCStar.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DoanCStar.ViewComponents
{
    public class PaymentMethodListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaymentViewModel vm)
        {
            // Value = mã, Text = nhãn hiển thị
            var methods = new[]
            {
                new { Value = "OFFLINE", Text = "Thanh toán tại quầy" },
                new { Value = "BANK",    Text = "Thẻ nội địa / Internet Banking" },
                new { Value = "CARD",    Text = "Visa / MasterCard" },
                new { Value = "WALLET",  Text = "Ví điện tử (Momo, ZaloPay, ...)" }
            };

            ViewBag.Methods = methods;
            return View(vm);
        }
    }
}
