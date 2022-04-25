using Grpc.Core;

namespace Api.Services;

public class NotFound : RpcException
{
    public NotFound(string message) : base(new Status(StatusCode.NotFound, message))
    {
    }
}

public class FailedPrecondition : RpcException
{
    public FailedPrecondition(string message) : base(new Status(StatusCode.FailedPrecondition, message))
    {
    }
}

public class InvalidArgument : RpcException
{
    public InvalidArgument(string message) : base(new Status(StatusCode.InvalidArgument, message))
    {
    }
}
