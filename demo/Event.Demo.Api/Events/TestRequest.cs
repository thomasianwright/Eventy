using Eventy.Abstractions.Events.Consumers;
using Eventy.Abstractions.Events.Contexts;
using Eventy.Abstractions.Events.Contracts;
using Eventy.Abstractions.Transports.Services;
using Eventy.Core.Events.Attributes;
using FluentResults;

namespace Event.Demo.Api.Events;

[EventTopology("test.request")]
public class TestRequest : IEvent
{
    public Guid CorrelationId { set;get; }
    
    public string Message { set; get; }
}

public class TestResponse
{
    public string Message { set; get; }
}

public class TestResponseHandler : IConsumer<TestRequest>
{
    public TestResponseHandler(ILogger<TestResponseHandler> logger)
    {
        
    }
    
    public async Task<Result> ConsumeAsync(TestRequest @event, CancellationToken cancellationToken = default)
    {
        await Context.RespondAsync(new TestResponse()
        {
            Message = $"SERVER: {@event.Message}"
        });
        
        return Result.Ok();
    }

    public IBus Bus { get; set; }
    public IEventContext Context { get; set; }
}