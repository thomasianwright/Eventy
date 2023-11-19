using Eventy.Events.Attributes;
using Eventy.Events.Consumers;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.Transports.Services;
using FluentResults;

namespace Event.Demo.Api.Events;

[EventTopology("test.request")]
public class TestRequest : IRequest
{
    public string CorrelationId { set;get; }
    
    public string Message { set; get; }
}

public class TestResponse
{
    public string Message { set; get; }
}

public class TestResponseHandler : IConsumer<TestRequest>
{
    private readonly ILogger<TestResponseHandler> _logger;

    public TestResponseHandler(ILogger<TestResponseHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task<Result> ConsumeAsync(TestRequest @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"CONSUMED: {@event.Message}");
        
        await Context.RespondAsync(new TestResponse()
        {
            Message = $"SERVER: {@event.Message}"
        });
        
        return Result.Ok();
    }

    public IBus Bus { get; set; }
    public IEventContext Context { get; set; }
}