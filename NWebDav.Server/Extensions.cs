using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NWebDav.Server.Authentication;
using NWebDav.Server.Handlers;
using NWebDav.Server.Helpers;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;

namespace NWebDav.Server;

public static class Extensions
{
    public static IServiceCollection AddNWebDav(this IServiceCollection services, Action<NWebDavOptions>? configureOptions = null)
    {
        services
            .AddHttpContextAccessor()
            .AddScoped<IXmlReaderWriter, XmlReaderWriter>()
            .AddTransient<IHandlerFactory, HandlerFactory>()
            .AddSingleton<ILockingManager, InMemoryLockingManager>();

        var handlerTypes = typeof(GetHandler).Assembly.DefinedTypes.Where(type =>
            type.ImplementsInterface<IRequestHandler>() && !type.IsAbstract).ToList();

        foreach (var handlerType in handlerTypes)
        {
            var keyName = handlerType.Name.Replace("Handler", string.Empty).ToUpperInvariant();
            services.AddKeyedScoped(typeof(IRequestHandler), keyName, handlerType);
        }

        var optionsBuilder = services
            .AddOptions<NWebDavOptions>()
            .BindConfiguration(NWebDavOptions.SectionName);
        //.Validate(o => o.Handlers.All(h => h.Key.ToUpperInvariant() == h.Key), "Handler methods should be uppercase");

        //var methods = new[] { "COPY", "DELETE", "GET", "HEAD", "MKCOL", "MOVE", "OPTIONS", "PROPFIND", "PROPPATCH", "PUT", "UNLOCK" };
        //foreach (var method in methods)
        //{
        //    optionsBuilder
        //        .Validate(o => o.Handlers.TryGetValue(method, out _), $"No handler for '{method}'")
        //        .Validate(o => !o.Handlers.TryGetValue(method, out var handlerType) || typeof(IRequestHandler).IsAssignableFrom(handlerType), $"Handler for '{method}' doesn't implement {nameof(IRequestHandler)}");
        //}

        services.Configure<NWebDavOptions>(opts =>
        {
            configureOptions?.Invoke(opts);
        });

        return services;
    }

    public static IServiceCollection AddStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TStore>(this IServiceCollection services) where TStore : class, IStore
        => services.AddScoped<IStore, TStore>();

    public static IServiceCollection AddDiskStore(this IServiceCollection services, Action<DiskStoreOptions>? configure = null)
        => services
            .Configure<DiskStoreOptions>(opts =>
            {
                opts.BaseDirectory = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("USERPROFILE") ?? string.Empty;
                configure?.Invoke(opts);
            })
            .AddDiskStore<DiskStore>();

    public static IServiceCollection AddDiskStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDiskStore>(this IServiceCollection services)
        where TDiskStore : DiskStoreBase
        => services
            .AddSingleton<DiskStoreCollectionPropertyManager>()
            .AddSingleton<DiskStoreItemPropertyManager>()
            .AddStore<TDiskStore>();

    public static IApplicationBuilder UseNWebDav(this IApplicationBuilder app)
    {
        //var opts = app.ApplicationServices.GetRequiredService<IOptions<NWebDavOptions>>();
        return app.UseMiddleware<NWebDavMiddleware>();
    }
}

public static class BasicAuthenticationExtensions
{
    public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder)
        => builder.AddBasicAuthentication(BasicAuthenticationDefaults.AuthenticationScheme, null);

    public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder, Action<BasicAuthenticationOptions> configureOptions)
        => builder.AddBasicAuthentication(BasicAuthenticationDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<BasicAuthenticationOptions>? configureOptions)
        => builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(authenticationScheme, configureOptions);
}

public static class ReflectionExtensions
{
    public static bool ImplementsInterface(this Type type, Type interfaceType)
    {
        if (interfaceType.IsGenericType)
        {
            var res = type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
            return res;
        }
        return interfaceType.IsAssignableFrom(type);
    }

    public static bool ImplementsInterface<T>(this Type type)
    {
        var interfaceType = typeof(T);
        return ImplementsInterface(type, interfaceType);
    }
}