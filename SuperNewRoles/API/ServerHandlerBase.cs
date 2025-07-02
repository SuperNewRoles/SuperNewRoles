using System.Net;

namespace SuperNewRoles.API;

public abstract class ServerHandlerBase
{
    public abstract string Path { get; }
    public abstract void Handle(HttpListenerContext context);
}
