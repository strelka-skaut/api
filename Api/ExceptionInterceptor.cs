using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Api;

public class ExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation
    )
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RpcException(new Status(StatusCode.Internal, e.ToString()));
        }
    }
}
