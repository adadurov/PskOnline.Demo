namespace PskOnline.Server.Service.Helpers
{
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Newtonsoft.Json.Linq;
  using PskOnline.Server.Shared.Service;
  using RestSharp;
  using RestSharp.Authenticators;
  using System;
  using System.Threading.Tasks;

  public class MailGunEmailService : IEmailService
  {
    private readonly ILogger<MailGunEmailService> _logger;
    private readonly RestClient _rsClient;

    private readonly string _mailGunApiKey;
    private readonly string _mailGunDomain;
    private readonly string _mailGunFrom;
    private readonly string _mailGunReplyTo;

    public MailGunEmailService(IConfiguration configuration, ILogger<MailGunEmailService> logger)
    {
      _logger = logger;
      _mailGunApiKey = configuration["MailGun:ApiKey"];
      _mailGunDomain = configuration["MailGun:Domain"];
      _mailGunFrom = configuration["Email:From"];
      _mailGunReplyTo = configuration["Email:ReplyTo"];

      _rsClient = new RestClient($"https://api.mailgun.net/v3");
      _rsClient.Authenticator = new HttpBasicAuthenticator("api", _mailGunApiKey);
    }

    public async Task SendEmailWithTemplateAsync(
      string templateName, string recipientName, string recipientEmail, string subject, object templateParams)
    {
      var request = new RestRequest("{domain}/messages", Method.POST);
      request.AddParameter("domain", _mailGunDomain, ParameterType.UrlSegment);
      request.AddParameter("from", _mailGunFrom);
      request.AddParameter("to", recipientEmail);
      request.AddParameter("subject", subject);
      request.AddParameter("template", templateName);
      request.AddParameter("h:Reply-To", _mailGunReplyTo);

      if (templateParams != null)
      {
        foreach (var property in templateParams.GetType().GetProperties())
        {
          request.AddParameter("v:" + property.Name, property.GetValue(templateParams) ?? "" );
        }
      }
      var response = await _rsClient.ExecuteTaskAsync(request);
      if (!response.IsSuccessful)
      {
        JObject error = null;
        try
        {
          error = JObject.Parse(response.Content);
        }
        catch (Exception ex)
        {
          _logger.LogError($"Cannot parse response from Mailgun as JSON.", ex);
        }

        var fullMsg = $"Request to send mail via Mailgun returned {response.StatusCode}. " +
                       "Error message: " + error?["message"] ?? "";
        _logger.LogError(fullMsg);

        throw new Exception("Could not send email.", new Exception(fullMsg));
      }
      _logger.LogInformation("Request to send email via Mailgun returned " + response.StatusCode);
    }
  }
}
