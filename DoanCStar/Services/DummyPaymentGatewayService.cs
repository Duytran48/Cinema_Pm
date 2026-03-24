using System.Threading.Tasks;
using DoanCStar.Models;

namespace DoanCStar.Services
{
    /// <summary>
    /// Cổng thanh toán giả lập: chỉ redirect lại returnUrl với kết quả giả.
    /// </summary>
    public class DummyPaymentGatewayService : IPaymentGatewayService
    {
        public Task<string> CreatePaymentUrlAsync(
            Payment payment,
            string returnUrl,
            string notifyUrl)
        {
            // Ở đây có thể sign query, thêm checksum...
            // Demo: giả sử luôn "success"
           var sep = returnUrl.Contains("?") ? "&" : "?";
            var url = $"{returnUrl}{sep}result=success";
            return Task.FromResult(url);
        }
    }
}
