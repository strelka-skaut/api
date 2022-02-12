using Api.Data;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

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
        Guid? siteId = null;
        if (request.SiteId != null)
        {
            siteId = Guid.Parse(request.SiteId.Value);
            if (!await _db.Sites.AnyAsync(s => s.Id == siteId))
                throw new Exception($"Site {siteId} does not exist.");
        }

        var id = Guid.NewGuid();

        _db.Pages.Add(new Data.Page
        {
            Id = id,
            Name = request.Name,
            Content = request.Content,
            UpdatedAt = DateTime.UtcNow,
            UpdatedUserId = Guid.Empty, // todo
            SiteId = siteId
        });

        await _db.SaveChangesAsync();

        return new PageServiceCreatePageResponse
        {
            Id = id.ToUuid()
        };
    }

    public override async Task<PageServiceGetPageResponse> GetPage(
        PageServiceGetPageRequest request,
        ServerCallContext context
    )
    {
        var id = Guid.Parse(request.PageId.Value);

        var dbPage = await _db.Pages.FindAsync(id);
        if (dbPage == null)
            throw new NotFound($"Page {id} not found.");

        return new PageServiceGetPageResponse
        {
            Page = new Page
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

    public override async Task<PageServiceGetPagesResponse> GetPages(
        PageServiceGetPagesRequest request,
        ServerCallContext context
    )
    {
        var dbPages = await _db.Pages.ToListAsync();

        var resp = new PageServiceGetPagesResponse();
        foreach (var page in dbPages)
        {
            resp.Pages.Add(new Page
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

    public override async Task<PageServiceDeletePageResponse> DeletePage(
        PageServiceDeletePageRequest request,
        ServerCallContext context
    )
    {
        var id = Guid.Parse(request.PageId.Value);
        var page = await _db.Pages.FindAsync(id);
        if (page == null)
            throw new NotFound($"Page {id} not found.");

        _db.Remove(page);
        await _db.SaveChangesAsync();

        return new PageServiceDeletePageResponse();
    }
}
