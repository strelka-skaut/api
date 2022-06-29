namespace Api.Handler;

public interface IHandler<in TRequest, TResponse>
{
    public Task<TResponse> Execute(TRequest request);
}
