namespace PskOnline.Server.Shared.Service
{
  using System.Threading.Tasks;

  public interface IEmailService
  {
    Task SendEmailWithTemplateAsync(
      string templateName, string recipientName, string recipientEmail, string subject, object templateParams);
  }
}
