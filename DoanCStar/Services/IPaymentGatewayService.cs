using System.Threading.Tasks;
using DoanCStar.Models;

namespace DoanCStar.Services
{
    public interface IPaymentGatewayService
    {
        /// <summary>
        /// Tạo URL thanh toán cho booking/payment hiện tại.
        /// </summary>
        Task<string> CreatePaymentUrlAsync(
            Payment payment,
            string returnUrl,   // URL user quay về sau khi thanh toán
            string notifyUrl    // URL cổng gọi server-to-server (nếu dùng)
        );
    }
}