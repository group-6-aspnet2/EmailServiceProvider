using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApi.Models;

namespace WebApi.Services;

public interface IQueueService
{
    Task StartAsync();
    Task StopAsync();
}

public class QueueService : IQueueService
{
    
    private readonly ServiceBusClient _client;    
    private readonly AzureServiceBusSettings _settings;
    private readonly ServiceBusProcessor _processor;
    private readonly IEmailService _emailService;


    public QueueService(IOptions<AzureServiceBusSettings> options, IEmailService emailService)
    {

        _settings = options.Value;
        _client = new ServiceBusClient(_settings.ConnectionString);
        _processor = _client.CreateProcessor(_settings.QueueName, new ServiceBusProcessorOptions());
        _emailService = emailService;

        RegisterMessageHandler();
        RegisterErrorHandler();
    }

    private void RegisterMessageHandler()
    {
        _processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var body = args.Message.Body.ToString();

                var emailSendRequest = JsonConvert.DeserializeObject<EmailSendRequest>(body) ?? throw new Exception("Unable to deserialize request");
                

                var result = await _emailService.SendEmailAsync(emailSendRequest);
                if (result)
                    await args.CompleteMessageAsync(args.Message);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Message handling failed: {ex.Message}");
                await args.AbandonMessageAsync(args.Message);
            }
        };
    }

    private void RegisterErrorHandler()
    {
        _processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine($"Error processing message: {args.Exception.Message}");
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync() => await _processor.StartProcessingAsync();


    public async Task StopAsync() => await _processor.StopProcessingAsync();
   

}
