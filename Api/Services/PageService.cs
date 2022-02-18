using ApiSpec.Grpc.Pages;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class PageService : Service.ServiceBase
{
    private readonly Data.MainDb          _db;
    private readonly ILogger<PageService> _log;

    public PageService(
        Data.MainDb          db,
        ILogger<PageService> log
    )
    {
        _db  = db;
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
            siteId = request.SiteId.ToGuid();
            if (!await _db.Sites.AnyAsync(s => s.Id == siteId))
                throw new Exception($"Site {siteId} does not exist.");
        }

        var id = Guid.NewGuid();

        _db.Pages.Add(new Data.Page
        {
            Id            = id,
            Name          = request.Name,
            Slug          = request.Slug,
            Content       = request.Content,
            UpdatedAt     = DateTime.UtcNow,
            UpdatedUserId = Guid.Empty, // todo
            SiteId        = siteId,
        });

        await _db.SaveChangesAsync();

        return new CreatePageResponse
        {
            Id = id.ToUuid(),
        };
    }

    public override async Task<GetPageResponse> GetPage(GetPageRequest request, ServerCallContext context)
    {
        var id = request.PageId.ToGuid();

        var dbPage = await _db.Pages.FindAsync(id);
        if (dbPage == null)
            throw new NotFound($"Page {id} not found.");

        return new GetPageResponse
        {
            Page = new Page
            {
                Id            = dbPage.Id.ToUuid(),
                Name          = dbPage.Name,
                Slug          = dbPage.Slug,
                Content       = dbPage.Content,
                UpdatedAt     = dbPage.UpdatedAt.ToTimestamp(),
                UpdatedUserId = dbPage.UpdatedUserId.ToUuid(),
                SiteId        = dbPage.SiteId?.ToUuid(),
            },
        };
    }

    public override async Task<GetPageBySlugResponse> GetPageBySlug(
        GetPageBySlugRequest request,
        ServerCallContext    context
    )
    {
        var page = await _db.Pages.FirstAsync(p => p.Slug == request.PageSlug);
        return new GetPageBySlugResponse
        {
            Page = new Page
            {
                Id            = page.Id.ToUuid(),
                Name          = page.Name,
                Slug          = page.Slug,
                Content       = page.Content,
                UpdatedAt     = page.UpdatedAt.ToTimestamp(),
                UpdatedUserId = page.UpdatedUserId.ToUuid(),
                SiteId        = page.SiteId?.ToUuid(),
            },
        };
    }

    public override async Task<GetPagesResponse> GetPages(
        GetPagesRequest   request,
        ServerCallContext context
    )
    {
        if (request.Limit > 1000)
            throw new Exception("Limit cannot be greater than 1000.");

        var dbPages = await _db.Pages
            .OrderBy(x => x.Content)
            .Skip(request.HasOffset ? request.Offset : 0)
            .Take(request.HasLimit ? request.Limit : 25)
            .ToListAsync();

        var resp = new GetPagesResponse();
        foreach (var page in dbPages)
        {
            resp.Pages.Add(new Page
            {
                Id            = page.Id.ToUuid(),
                Name          = page.Name,
                Slug          = page.Slug,
                Content       = page.Content,
                UpdatedAt     = page.UpdatedAt.ToTimestamp(),
                UpdatedUserId = page.UpdatedUserId.ToUuid(),
                SiteId        = page.SiteId?.ToUuid(),
            });
        }

        return resp;
    }

    public override async Task<UpdatePageResponse> UpdatePage(UpdatePageRequest request, ServerCallContext context)
    {
        var id = request.PageId.ToGuid();

        var dbPage = await _db.Pages.FindAsync(id);
        if (dbPage == null)
            throw new NotFound($"Page {id} not found.");

        if (request.HasName)
            dbPage.Name = request.Name;

        if (request.HasSlug)
            dbPage.Slug = request.Slug;

        if (request.HasContent)
            dbPage.Content = request.Content;

        if (request.HasSiteId)
            dbPage.SiteId = request.SiteId.ToGuid();

        dbPage.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new UpdatePageResponse();
    }

    public override async Task<DeletePageResponse> DeletePage(
        DeletePageRequest request,
        ServerCallContext context
    )
    {
        var id = request.PageId.ToGuid();
        var page = await _db.Pages.FindAsync(id);
        if (page == null)
            throw new NotFound($"Page {id} not found.");

        _db.Remove(page);
        await _db.SaveChangesAsync();

        return new DeletePageResponse();
    }
}
