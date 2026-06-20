namespace KX_Project.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
