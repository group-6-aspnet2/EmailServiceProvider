using Azure.Communication.Email;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Options;
using WebApi.Models;


namespace WebApi.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailSendRequest emailSendRequst);    
}

public class EmailService : IEmailService
{
    private readonly AzureCommunicationSettings _settings;
    private readonly EmailClient _client;

    public EmailService(IOptions<AzureCommunicationSettings>options)
    {
        _settings = options.Value;
        _client = new EmailClient(_settings.ConnectionString);
    }
    

    public async Task<bool> SendEmailAsync(EmailSendRequest emailSendRequest)
    {
        var recipients = new List<EmailAddress>();

        foreach (var recipient in emailSendRequest.Recipients)
            recipients.Add(new EmailAddress(recipient));

        var message = new EmailMessage(
        senderAddress: _settings.SenderAddress,
        content: new EmailContent(emailSendRequest.Subject)
        {
            PlainText = emailSendRequest.PlainText,
            Html = emailSendRequest.Html
        },
        recipients: new EmailRecipients(recipients));

       var result = await _client.SendAsync(Azure.WaitUntil.Completed, message);
       return result.HasCompleted;
    }
    
}