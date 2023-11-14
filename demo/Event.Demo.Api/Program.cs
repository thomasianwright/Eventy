using Event.Demo.Api;
using Event.Demo.Api.Events;
using Eventy.Abstractions.Events.Encoders;
using Eventy.Abstractions.IoC.Services;
using Eventy.Abstractions.Transports.Services;
using Eventy.Core.Events.Encoders;
using Eventy.Transports.RabbitMQ;
using Eventy.Transports.RabbitMQ.Contracts;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionFactory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    Port = 5672,
    VirtualHost = "/",
    DispatchConsumersAsync = true,
    AutomaticRecoveryEnabled = true,

    // Configure the amount of concurrent consumers within one host
    ConsumerDispatchConcurrency = 10
};

var conn =  connectionFactory.CreateConnection();

builder.Services.AddSingleton<IConnection>(conn);

builder.Services.AddSingleton<IEventEncoder, EventEncoder>()
    .AddSingleton<IServiceResolver, ServiceResolver>();

builder.Services
    .AddSingleton<IRabbitMqTransportProvider, RabbitMqTransportProvider>()
    .AddSingleton<ITransportProvider>(sp => sp.GetRequiredService<IRabbitMqTransportProvider>())
    .AddSingleton<IBus>(sp => sp.GetRequiredService<IRabbitMqTransportProvider>())
    .AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<IRabbitMqTransportProvider>());

builder.Services.AddScoped<TestEventConsumer>()
    .AddScoped<TestResponseHandler>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();


var transportProvider = app.Services.GetRequiredService<ITransportProvider>();

transportProvider.AddEventTypes(typeof(TestEvent), typeof(TestRequest));
transportProvider.AddConsumers(typeof(TestEventConsumer), typeof(TestResponseHandler));

await transportProvider.Start();

app.Run();