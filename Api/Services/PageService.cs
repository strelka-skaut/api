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
                Role          = dbPage.Role,
                UpdatedAt     = dbPage.UpdatedAt.ToTimestamp(),
                UpdatedUserId = dbPage.UpdatedUserId.ToUuid(),
                SiteId        = dbPage.SiteId.ToUuid(),
                ParentId      = dbPage.ParentId?.ToUuid(),
            },
        };
    }

    public override async Task<GetPageBySiteAndPathResponse> GetPageBySiteAndPath(
        GetPageBySiteAndPathRequest request,
        ServerCallContext           context
    )
    {
        var siteId = request.SiteId.ToGuid();

        Data.Page? page = null;

        foreach (var part in request.Path.Trim('/').Split('/'))
        {
            page = await _db.Pages.FirstOrDefaultAsync(p =>
                p.SiteId == siteId
                && p.ParentId == (page != null ? page.Id : null)
                && p.Slug == part
            );

            if (page == null)
                throw new NotFound("Page not found.");
        }

        return new GetPageBySiteAndPathResponse
        {
            Page = new Page
            {
                Id            = page.Id.ToUuid(),
                Name          = page.Name,
                Slug          = page.Slug,
                Content       = page.Content,
                Role          = page.Role,
                UpdatedAt     = page.UpdatedAt.ToTimestamp(),
                UpdatedUserId = page.UpdatedUserId.ToUuid(),
                SiteId        = page.SiteId.ToUuid(),
                ParentId      = page.ParentId?.ToUuid(),
            },
        };
    }

    public override async Task<GetPagesResponse> GetPages(
        GetPagesRequest   request,
        ServerCallContext context
    )
    {
        if (request.Limit > 1000)
            throw new InvalidArgument("Limit cannot be greater than 1000.");

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
                Role          = page.Role,
                UpdatedAt     = page.UpdatedAt.ToTimestamp(),
                UpdatedUserId = page.UpdatedUserId.ToUuid(),
                SiteId        = page.SiteId.ToUuid(),
                ParentId      = page.ParentId?.ToUuid(),
            });
        }

        return resp;
    }

    public override async Task<CreatePageResponse> CreatePage(
        CreatePageRequest request,
        ServerCallContext context
    )
    {
        if (request.SiteId == null)
            throw new InvalidArgument("SiteId is required.");

        var siteId = request.SiteId.ToGuid();
        if (!await _db.Sites.AnyAsync(s => s.Id == siteId))
            throw new FailedPrecondition($"Site {siteId} does not exist.");

        Guid? parentId = null;
        if (request.ParentId != null)
        {
            parentId = request.ParentId.ToGuid();
            if (!await _db.Pages.AnyAsync(p => p.Id == parentId))
                throw new FailedPrecondition($"Page {parentId} does not exist.");
        }
        // todo validation: loop, roles...

        if (!Validators.IsValidSlug(request.Slug))
            throw new InvalidArgument("Invalid format of slug.");

        var id = Guid.NewGuid();

        _db.Pages.Add(new Data.Page
        {
            Id            = id,
            Name          = request.Name,
            Slug          = request.Slug,
            Content       = request.Content,
            Role          = request.Role,
            UpdatedAt     = DateTime.UtcNow,
            UpdatedUserId = Guid.Empty, // todo
            SiteId        = siteId,
            ParentId      = parentId,
        });

        await _db.SaveChangesAsync();

        return new CreatePageResponse
        {
            Id = id.ToUuid(),
        };
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
        {
            if (!Validators.IsValidSlug(request.Slug))
                throw new InvalidArgument("Invalid format of slug.");

            dbPage.Slug = request.Slug;
        }

        if (request.HasContent)
            dbPage.Content = request.Content;

        if (request.HasRole)
            dbPage.Role = request.Role;

        if (request.SiteId != null)
        {
            var siteId = request.SiteId.ToGuid();
            if (!await _db.Sites.AnyAsync(s => s.Id == siteId))
                throw new FailedPrecondition($"Site {siteId} does not exist.");

            dbPage.SiteId = siteId;
        }

        if (request.HasParentId)
            dbPage.ParentId = request.ParentId.ToGuid(); // todo validation

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

        // todo remove nested pages

        return new DeletePageResponse();
    }
}
