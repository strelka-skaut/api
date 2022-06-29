using System.Text.RegularExpressions;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Handler;

public class GetPageByPath : IHandler<GetPageByPath.Query, GetPageByPath.Result>
{
    public record Query(string Path, Guid SiteId);

    public record Result(Page? Page);

    private readonly MainDb _db;

    public GetPageByPath(MainDb db)
    {
        _db = db;
    }

    public async Task<Result> Execute(Query query)
    {
        Page? page = null;

        foreach (var part in query.Path.Trim('/').Split('/'))
        {
            page = await GetPage(query.SiteId, part, page?.Id);

            if (page == null)
                break;
        }

        return new(page);
    }

    private async Task<Page?> GetPage(Guid siteId, string slug, Guid? parentId)
    {
        return await GetPageByExactMatch(siteId, slug, parentId)
               ?? await GetPageUsingWildcard(siteId, slug, parentId)
               ?? null;
    }

    private async Task<Page?> GetPageByExactMatch(Guid siteId, string slug, Guid? parentId)
    {
        return await _db.Pages.FirstOrDefaultAsync(p => p.SiteId == siteId && p.ParentId == parentId && p.Slug == slug);
    }

    private async Task<Page?> GetPageUsingWildcard(Guid siteId, string slug, Guid? parentId)
    {
        var pages = _db.Pages
            .Where(p => p.SiteId == siteId && p.ParentId == parentId && p.Slug.IndexOf("{") >= 0)
            .ToList();

        foreach (var page in pages)
        {
            if (Regex.IsMatch(page.Slug, "^[a-z0-9-]*\\{event:slug\\}[a-z0-9-]*$"))
            {
                var pattern = "^" + page.Slug.Replace("{event:slug}", "(?<slug>.+)") + "$";
                var matches = Regex.Matches(slug, pattern);
                if (matches.Count != 1)
                    continue;

                var eventSlug = matches.First().Groups["slug"].Value;
                var ev = await _db.Events.FirstOrDefaultAsync(e => e.Slug == eventSlug);
                if (ev != null)
                    return page;
            }
            else
                throw new Exception($"Invalid slug: {page.Slug}");
        }

        return null;
    }
}
