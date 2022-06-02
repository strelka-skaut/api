using ApiSpec.Grpc.Sites;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class SiteService : Service.ServiceBase
{
    private readonly Data.MainDb          _db;
    private readonly ILogger<SiteService> _log;

    public SiteService(
        Data.MainDb          db,
        ILogger<SiteService> log
    )
    {
        _db  = db;
        _log = log;
    }

    public override async Task<GetSiteResponse> GetSite(GetSiteRequest request, ServerCallContext context)
    {
        var id = request.SiteId.ToGuid();

        var dbSite = await _db.Sites.FindAsync(id);
        if (dbSite == null)
            throw new NotFound($"Site {id} not found.");

        return new GetSiteResponse
        {
            Site = new Site
            {
                Id   = dbSite.Id.ToUuid(),
                Name = dbSite.Name,
                Slug = dbSite.Slug,
            },
        };
    }

    public override async Task<GetSiteBySlugResponse> GetSiteBySlug(
        GetSiteBySlugRequest request,
        ServerCallContext    context
    )
    {
        var site = await _db.Sites.FirstOrDefaultAsync(p => p.Slug == request.SiteSlug);
        if (site == null)
            throw new NotFound("Site not found.");

        return new GetSiteBySlugResponse
        {
            Site = new Site
            {
                Id   = site.Id.ToUuid(),
                Name = site.Name,
                Slug = site.Slug,
            },
        };
    }

    public override async Task<GetSitesResponse> GetSites(
        GetSitesRequest   request,
        ServerCallContext context
    )
    {
        if (request.Limit > 1000)
            throw new InvalidArgument("Limit cannot be greater than 1000.");

        var dbSites = await _db.Sites
            .OrderBy(x => x.Id)
            .Skip(request.HasOffset ? request.Offset : 0)
            .Take(request.HasLimit ? request.Limit : 25)
            .ToListAsync();

        var resp = new GetSitesResponse();
        foreach (var site in dbSites)
        {
            resp.Sites.Add(new Site
            {
                Id   = site.Id.ToUuid(),
                Name = site.Name,
                Slug = site.Slug,
            });
        }

        return resp;
    }

    public override async Task<CreateSiteResponse> CreateSite(
        CreateSiteRequest request,
        ServerCallContext context
    )
    {
        if (!Validators.IsValidSlug(request.Slug))
            throw new InvalidArgument("Invalid format of slug.");

        var id = Guid.NewGuid();

        _db.Sites.Add(new Data.Site
        {
            Id   = id,
            Name = request.Name,
            Slug = request.Slug,
        });

        await _db.SaveChangesAsync();

        return new CreateSiteResponse
        {
            Id = id.ToUuid(),
        };
    }

    public override async Task<UpdateSiteResponse> UpdateSite(UpdateSiteRequest request, ServerCallContext context)
    {
        var id = request.SiteId.ToGuid();

        var dbSite = await _db.Sites.FindAsync(id);
        if (dbSite == null)
            throw new NotFound($"Site {id} not found.");

        if (request.HasName)
            dbSite.Name = request.Name;

        if (request.HasSlug)
        {
            if (!Validators.IsValidSlug(request.Slug))
                throw new InvalidArgument("Invalid format of slug.");

            dbSite.Slug = request.Slug;
        }

        await _db.SaveChangesAsync();

        return new UpdateSiteResponse();
    }

    public override async Task<DeleteSiteResponse> DeleteSite(
        DeleteSiteRequest request,
        ServerCallContext context
    )
    {
        var id = request.SiteId.ToGuid();
        var site = await _db.Sites.FindAsync(id);
        if (site == null)
            throw new NotFound($"Site {id} not found.");

        _db.Remove(site);
        await _db.SaveChangesAsync();

        return new DeleteSiteResponse();
    }
}
