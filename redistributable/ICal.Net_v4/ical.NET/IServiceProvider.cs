using System;

namespace Ical.Net
{
    public interface IServiceProvider
    {
        object GetService(string name);
        object GetService(Type type);
        T GetService<T>();
        T GetService<T>(string name);
        void SetService(string name, object obj);
        void SetService(object obj);
        void RemoveService(Type type);
        void RemoveService(string name);
    }
}