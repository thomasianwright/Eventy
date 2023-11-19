using Eventy.Events.Attributes;
using Eventy.Events.Consumers;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.Transports.Services;
using FluentResults;

namespace Event.Demo.Api.Events;

[EventTopology("test-event")]
public class TestEvent : IEvent
{
    public TestEvent()
    {
        CorrelationId = Guid.NewGuid().ToString();
    }

    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CorrelationId { get; set; }
}

public class TestEventConsumer : IConsumer<TestEvent>
{
    private readonly ILogger<TestEventConsumer> _logger;

    public TestEventConsumer(ILogger<TestEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task<Result> ConsumeAsync(TestEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            $"Received event {@event.CorrelationId} with message {@event.Message} TIME: {DateTimeOffset.UtcNow}");
        
        return Task.FromResult(Result.Ok());
    }

    public IBus Bus { get; set; }
    public IEventContext Context { get; set; }
}