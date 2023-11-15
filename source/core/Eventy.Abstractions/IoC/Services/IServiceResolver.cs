using System;

namespace Eventy.IoC.Services
{
    public interface IServiceResolver : IDisposable
    {
        T GetService<T>();

        T GetService<T>(Type type);

        object GetService(Type type);

        IServiceResolver CreateScope();
    }
}