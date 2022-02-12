using Grpc.Core;

namespace Api.Services;

public class NotFound : RpcException
{
    public NotFound(string message) : base(new Status(StatusCode.NotFound, message))
    {
    }
}
