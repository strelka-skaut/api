using Api.Data;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Api.Services;

public class PageService : Api.PageService.PageServiceBase
{
    private MainDb _db;
    private readonly ILogger<PageService> _log;

    public PageService(
        MainDb db,
        ILogger<PageService> log
    )
    {
        _db = db;
        _log = log;
    }

    public override async Task<PageServiceCreatePageResponse> CreatePage(
        PageServiceCreatePageRequest request,
        ServerCallContext context
    )
    {
        Site? site = null;
        if (request.SiteId != null)
        {
            site = await _db.Sites.FindAsync(request.SiteId);
            if (site == null)
                throw new NotFound(); // todo
        }

        var id = Guid.NewGuid();

        _db.Pages.Add(new Data.Page
        {
            Id = id,
            Name = request.Name,
            Content = request.Content,
            UpdatedAt = DateTime.UtcNow,
            UpdatedUserId = Guid.Empty, // todo
            Site = site
        });

        await _db.SaveChangesAsync();

        return new PageServiceCreatePageResponse
        {
            Id = new Uuid {Value = id.ToString()}
        };
    }

    public override async Task<PageServiceGetPageResponse> GetPage(
        PageServiceGetPageRequest request,
        ServerCallContext context
    )
    {
        var dbPage = await _db.Pages.FindAsync(Guid.Parse(request.PageId.Value));
        if (dbPage == null)
            throw new NotFound(); // todo

        return new PageServiceGetPageResponse
        {
            Page = new Page
            {
                Id = new Uuid {Value = dbPage.Id.ToString()},
                Name = dbPage.Name,
                Content = dbPage.Content,
                UpdatedAt = dbPage.UpdatedAt.ToTimestamp(),
                UpdatedUserId = new Uuid {Value = dbPage.UpdatedUserId.ToString()},
                SiteId = dbPage.Site != null
                    ? new Uuid {Value = dbPage.Site.Id.ToString()}
                    : null // todo dont query the Site row
            },
        };
    }
}
