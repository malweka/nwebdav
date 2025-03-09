using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace NWebDav.Server;

public record NWebDavOptions
{
    public const string SectionName = "NWebDav";

    public static readonly string[] DefaultAllowedMethods = new[] { "COPY", "DELETE", "GET", "HEAD", "MKCOL", "MOVE", "OPTIONS", "PROPFIND", "PROPPATCH", "PUT", "UNLOCK" };

    /// <summary>
    /// RequireAuthentication enforces that all requests -with the exception
    /// of <c>OPTIONS</c>- require the HTTP request to be authenticated.
    /// </summary>
    /// <remarks>
    /// If anonymous access is allowed, then set this property to <c>false</c>.
    /// The authentication middleware will still be invoked, but make sure it
    /// won't challenge the user to authenticate.
    /// </remarks>
    public bool RequireAuthentication { get; set; }

    public string[] AllowedMethods { get; init; } = new[] { "COPY", "DELETE", "GET", "HEAD", "MKCOL", "MOVE", "OPTIONS", "PROPFIND", "PROPPATCH", "PUT", "UNLOCK" };

    public string WebDavPathPrefix { get; set; } = "_dav";

    /// <summary>
    /// Filter allows to filter incoming HTTP requests. The default filter
    /// will pass all requests to the NWebDAV middleware.
    /// </summary>
    public Func<HttpContext, bool>? Filter { get; set; }

    public static bool IsAllowed(HttpContext ctx, string[] allowedMethods)
    {
        return allowedMethods.Contains(ctx.Request.Method);
    }
    
    ///// <summary>
    ///// Handlers maps the WebDAV methods to the handlers.
    ///// </summary>
    ///// <remarks>
    ///// The default handlers should be fine for most normal operations. If
    ///// you feel the need to use your own handler, then check if you can
    ///// accomplish the same using a custom store and/or properties.
    ///// </remarks>
    //public IDictionary<string, Type> Handlers { get; } = new Dictionary<string, Type>();
}