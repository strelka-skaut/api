using Api.Data;
using Api.Grpc.Pages;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Page = Api.Data.Page;

namespace Api.Services;

public class PageService : Service.ServiceBase
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

    public override async Task<CreatePageResponse> CreatePage(
        CreatePageRequest request,
        ServerCallContext context
    )
    {
        Guid? siteId = null;
        if (request.SiteId != null)
        {
            siteId = Guid.Parse(request.SiteId.Value);
            if (!await _db.Sites.AnyAsync(s => s.Id == siteId))
                throw new Exception($"Site {siteId} does not exist.");
        }

        var id = Guid.NewGuid();

        _db.Pages.Add(new Page
        {
            Id = id,
            Name = request.Name,
            Content = request.Content,
            UpdatedAt = DateTime.UtcNow,
            UpdatedUserId = Guid.Empty, // todo
            SiteId = siteId
        });

        await _db.SaveChangesAsync();

        return new CreatePageResponse
        {
            Id = id.ToUuid()
        };
    }

    public override async Task<GetPageResponse> GetPage(
        GetPageRequest request,
        ServerCallContext context
    )
    {
        var id = Guid.Parse(request.PageId.Value);

        var dbPage = await _db.Pages.FindAsync(id);
        if (dbPage == null)
            throw new NotFound($"Page {id} not found.");

        return new GetPageResponse
        {
            Page = new Grpc.Pages.Page
            {
                Id = dbPage.Id.ToUuid(),
                Name = dbPage.Name,
                Content = dbPage.Content,
                UpdatedAt = dbPage.UpdatedAt.ToTimestamp(),
                UpdatedUserId = dbPage.UpdatedUserId.ToUuid(),
                SiteId = dbPage.SiteId?.ToUuid()
            },
        };
    }

    public override async Task<GetPagesResponse> GetPages(
        GetPagesRequest request,
        ServerCallContext context
    )
    {
        if (request.Limit > 1000)
            throw new Exception("Limit cannot be greater than 1000.");

        var dbPages = await _db.Pages
            .Skip(request.HasOffset ? request.Offset : 0)
            .Take(request.HasLimit ? request.Limit : 25)
            .OrderBy(x => x.Content)
            .ToListAsync();

        var resp = new GetPagesResponse();
        foreach (var page in dbPages)
        {
            resp.Pages.Add(new Grpc.Pages.Page
            {
                Id = page.Id.ToUuid(),
                Name = page.Name,
                Content = page.Content,
                UpdatedAt = page.UpdatedAt.ToTimestamp(),
                UpdatedUserId = page.UpdatedUserId.ToUuid(),
                SiteId = page.SiteId?.ToUuid()
            });
        }

        return resp;
    }

    public override async Task<DeletePageResponse> DeletePage(
        DeletePageRequest request,
        ServerCallContext context
    )
    {
        var id = Guid.Parse(request.PageId.Value);
        var page = await _db.Pages.FindAsync(id);
        if (page == null)
            throw new NotFound($"Page {id} not found.");

        _db.Remove(page);
        await _db.SaveChangesAsync();

        return new DeletePageResponse();
    }
}
