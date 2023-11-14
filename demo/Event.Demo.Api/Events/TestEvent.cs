using Eventy.Abstractions.Events.Consumers;
using Eventy.Abstractions.Events.Contexts;
using Eventy.Abstractions.Events.Contracts;
using Eventy.Abstractions.Transports.Services;
using Eventy.Core.Events.Attributes;
using FluentResults;

namespace Event.Demo.Api.Events;

[EventTopology("test-event")]
public class TestEvent : IEvent
{
    public TestEvent()
    {
        CorrelationId = Guid.NewGuid();
    }

    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CorrelationId { get; set; }
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

        Context.Ack();

        return Task.FromResult(Result.Ok());
    }

    public IBus Bus { get; set; }
    public IEventContext Context { get; set; }
}