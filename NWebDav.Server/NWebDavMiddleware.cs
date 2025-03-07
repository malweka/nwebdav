using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NWebDav.Server.Handlers;

namespace NWebDav.Server;

internal class NWebDavMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NWebDavMiddleware> _logger;

    public NWebDavMiddleware(RequestDelegate next, ILogger<NWebDavMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<NWebDavOptions> options, IHandlerFactory handlerFactory)
    {
        var opts = options.Value;
        var requestPath = context.Request.Path.Value ?? string.Empty;

        // Check if the path starts with /__dav
        if (!requestPath.StartsWith($"/{opts.WebDavPathPrefix}", StringComparison.OrdinalIgnoreCase))
        {
            // Not a WebDAV request, pass to next middleware
            await _next(context).ConfigureAwait(false);
            return;
        }
        
        var filter = opts.Filter ?? (ctx => NWebDavOptions.IsAllowed(ctx, opts.AllowedMethods));

        if (filter(context))
        {
            _logger.LogTrace("Handling request for path '{Path}' with method '{HttpMethod}'.", context.Request.Path, context.Request.Method);
            var handler = handlerFactory.CreateHandler(context.Request.Method);
            if (handler != null)
            {
                var handled = await handler.HandleRequestAsync(context).ConfigureAwait(false);
                if (handled) return;
            }
            else
            {
                var response = context.Response;
                response.StatusCode = StatusCodes.Status501NotImplemented;
                response.ContentType = "text/plain";

                // Add the Allow header with supported methods
                var supportedMethods = string.Join(", ", opts.AllowedMethods);
                response.Headers.Append("Allow", supportedMethods);

                // Write message to response body
                var message = $"The requested method {context.Request.Method} is not implemented on this server.";
                await response.WriteAsync(message);

                _logger.LogTrace("Skipped request, because HTTP method {HttpMethod} has no handler.", context.Request.Method);

                // Important: Return here to prevent calling next middleware
                return;
            }
        }
        else
        {
            var response = context.Response;
            response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            response.ContentType = "text/plain";

            // Add the Allow header with supported methods
            var supportedMethods = string.Join(", ", opts.AllowedMethods);
            response.Headers.Append("Allow", supportedMethods);

            // Write message to response body
            var message = $"The method {context.Request.Method} is not allowed for this resource.";
            await response.WriteAsync(message);

            _logger.LogTrace("Skipped request, because it didn't match the filter.");

            // Important: Return here to prevent calling next middleware
            return;
        }

        // Default handling
        await _next(context).ConfigureAwait(false);
    }
}