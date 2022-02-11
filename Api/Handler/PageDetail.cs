namespace Api.Handler;

public class PageDetail : IHandler<PageDetail.Query, PageDetail.Result>
{
    public record Query(Guid Id) : IRequest<Result>;

    public record Result(Guid Id, string Name, string Content, Guid AuthorId, string AuthorName);

    public Result Execute(Query query)
    {
        return new Result(query.Id, "Podzimky 2021", "Sesli jsme se...", Guid.NewGuid(), "MartinH");
    }
}
