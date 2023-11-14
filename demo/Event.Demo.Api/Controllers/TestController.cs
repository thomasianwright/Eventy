using Event.Demo.Api.Events;
using Eventy.Abstractions.Transports.Services;
using Microsoft.AspNetCore.Mvc;

namespace Event.Demo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] IEventPublisher publisher)
    {
        var response = await publisher.RequestAsync(new TestRequest()
        {
            CorrelationId = Guid.NewGuid(),
            Message = "Hello World"
        });

        return Ok(response);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] IEventPublisher publisher, [FromQuery] string message, CancellationToken cancellationToken = default)
    {
        await publisher.PublishAsync(new TestEvent()
        {
            Message = message
        }, cancellationToken);
        
        return Ok();
    }
}