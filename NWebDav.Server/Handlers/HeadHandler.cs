using Microsoft.AspNetCore.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers;

public class HeadHandler : GetAndHeadBaseHandler
{
    public HeadHandler(IStore store) : base(store)
    {
    }
    public override string Method => HttpMethods.Head;
}