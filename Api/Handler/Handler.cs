namespace Api.Handler;

public interface IRequest<TResponse>
{
}

public interface IHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public TResponse Execute(TRequest request);
}
