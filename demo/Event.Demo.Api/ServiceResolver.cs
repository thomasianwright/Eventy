using Eventy.Abstractions.IoC.Services;

namespace Event.Demo.Api;

public class ServiceResolver : IServiceResolver
{
    private readonly IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;

    public ServiceResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Dispose()
    {
        _serviceScope?.Dispose();
    }

    public T GetService<T>()
    {
        return (T)_serviceProvider.GetService(typeof(T));
    }

    public T GetService<T>(Type type)
    {
        return (T)_serviceProvider.GetService(type);
    }

    public object GetService(Type type)
    {
        return _serviceProvider.GetService(type);
    }

    public IServiceResolver CreateScope()
    {
        _serviceScope = _serviceProvider.CreateScope();

        return new ServiceResolver(_serviceScope.ServiceProvider);
    }
}