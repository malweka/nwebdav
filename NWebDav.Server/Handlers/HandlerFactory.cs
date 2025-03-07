using System;
using Microsoft.Extensions.DependencyInjection;

namespace NWebDav.Server.Handlers;

public interface IHandlerFactory
{
    IRequestHandler? CreateHandler(string httpMethod);
}

public class HandlerFactory : IHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    public HandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IRequestHandler? CreateHandler(string httpMethod)
    {
        var handler = _serviceProvider.GetKeyedService<IRequestHandler>(httpMethod);
        return handler;
    }
}