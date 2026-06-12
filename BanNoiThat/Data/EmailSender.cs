using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace BanNoiThat.Data
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Đây là trình gửi email ảo phục vụ cho việc chạy thử nghiệm cục bộ (Development)
            // Không thực hiện gửi email thật, chỉ trả về hoàn thành tác vụ
            return Task.CompletedTask;
        }
    }
}
