using Microsoft.AspNetCore.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers;

public class GetHandler : GetAndHeadBaseHandler
{
    public GetHandler(IStore store) : base(store)
    {
    }
    public override string Method => HttpMethods.Get;
}