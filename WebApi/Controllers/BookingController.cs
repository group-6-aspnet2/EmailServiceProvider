using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingController(IOptions<AzureServiceBusSettings> options) : ControllerBase
{
    private readonly ServiceBusClient _client = new(options.Value.ConnectionString);
    private readonly string _queueName = options.Value.QueueName;

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookingConfirmationRequest request)
    {
        // Validering av input
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Skapa e-postmeddelande baserat på bokningsinfo
        var emailRequest = new EmailSendRequest
        {
            Recipients = [request.UserEmail], 
            Subject = "Bokningsbekräftelse",
            PlainText = $"Tack för din bokning! Ditt boknings-ID: {request.BookingId}",
            Html = $"<h1>Bokningsbekräftelse</h1><p>Ditt boknings-ID: <strong>{request.BookingId}</strong></p>"
        };

        var json = JsonConvert.SerializeObject(emailRequest);

        var sender = _client.CreateSender(_queueName);
        var message = new ServiceBusMessage(json);
        await sender.SendMessageAsync(message);

        return Ok(new { message = "Bokning mottagen och bekräftelse skickas." });
    }
}
