using Grpc.Core;

namespace Api.Services;

public class PageService : Api.PageService.PageServiceBase
{
    private readonly ILogger<PageService> _log;

    private readonly List<Page> _pages = new()
    {
        new Page
        {
            Id = 123,
            Title = "Podzimky 2021",
            Content = "Sesli jsme se...",
            AuthorId = "vitek@skaut.cz",
            CreatedAt = 183651124,
        },
        new Page
        {
            Id = 456,
            Title = "Silvestr 2021",
            Content = "Sesli jsme se opet...",
            AuthorId = "gwen@skaut.cz",
            CreatedAt = 123111225,
        }
    };

    public PageService(ILogger<PageService> log)
    {
        _log = log;
    }

    public override Task<PageServiceGetPageResponse> GetPage(
        PageServiceGetPageRequest request,
        ServerCallContext context
    )
    {
        return Task.FromResult(new PageServiceGetPageResponse
        {
            Page = _pages.First(),
        });
    }
}
