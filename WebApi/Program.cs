using WebApi.Models;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<AzureCommunicationSettings>(builder.Configuration.GetSection("AzureCommunicationSettings"));
builder.Services.Configure<AzureServiceBusSettings>(builder.Configuration.GetSection("AzureServiceBusSettings"));


builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IQueueService, QueueService>();

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var queueService = app.Services.GetRequiredService<IQueueService>();
await queueService.StartAsync();

app.Run();