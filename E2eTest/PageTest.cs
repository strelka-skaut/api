using Api.Data;
using ApiSpec.Grpc;
using ApiSpec.Grpc.Pages;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;
using PagesClient = ApiSpec.Grpc.Pages.Service.ServiceClient;
using EventsClient = ApiSpec.Grpc.Events.Service.ServiceClient;

namespace E2eTest;

public class PageTest : IClassFixture<Fixture>, IDisposable
{
    private readonly MainDb       _db;
    private readonly PagesClient  _pagesClient;
    private readonly EventsClient _eventsClient;

    private readonly Uuid _siteId;

    public PageTest(Fixture fixture)
    {
        _db           = fixture.Db;
        _pagesClient  = new PagesClient(fixture.Channel);
        _eventsClient = new EventsClient(fixture.Channel);

        var siteId = Guid.NewGuid();
        _db.Sites.Add(new Site
        {
            Id   = siteId,
            Name = "",
            Slug = "",
        });
        _db.SaveChanges();
        _siteId = new Uuid {Value = siteId.ToString()};
    }

    public void Dispose()
    {
        _db.Database.ExecuteSqlRaw("DELETE FROM \"Event\";");
        _db.Database.ExecuteSqlRaw("DELETE FROM \"Page\";");
        _db.Database.ExecuteSqlRaw("DELETE FROM \"Site\";");
    }

    [Fact]
    public void TestCreateAndGet()
    {
        var respCreate = _pagesClient.CreatePage(new()
        {
            Name    = "Silvestr 2021",
            Slug    = "silvestr-2021",
            Content = "Sešli jsme se...",
            SiteId  = _siteId,
            Role    = "event",
        });

        Assert.NotNull(respCreate.Id);

        var respGet = _pagesClient.GetPage(new() {PageId = respCreate.Id});

        Assert.Equal(respCreate.Id, respGet.Page.Id);
        Assert.Equal("Silvestr 2021", respGet.Page.Name);
        Assert.Equal("silvestr-2021", respGet.Page.Slug);
        Assert.Equal("Sešli jsme se...", respGet.Page.Content);
        Assert.Equal(_siteId, respGet.Page.SiteId);
        Assert.Equal("event", respGet.Page.Role);
        Assert.True(respGet.Page.UpdatedAt.ToDateTime().Year > 0);
        Assert.Null(respGet.Page.ParentId);
    }

    [Fact]
    public void TestCreateFailsWhenSiteDoesNotExist()
    {
        var request = new CreatePageRequest
        {
            SiteId = new Uuid {Value = "53ba52cc-0226-4093-9c43-8c873a9c11a2"},
        };

        var e = Assert.Throws<RpcException>(() => _pagesClient.CreatePage(request));
        Assert.Equal(StatusCode.FailedPrecondition, e.StatusCode);
    }

    [Fact]
    public void TestUpdateFailsWhenSiteDoesNotExist()
    {
        var respCreate = _pagesClient.CreatePage(new()
        {
            Slug   = "silvestr-2021",
            SiteId = _siteId,
        });

        var id = respCreate.Id;

        var updateRequest = new UpdatePageRequest
        {
            PageId = id,
            SiteId = new Uuid {Value = "53ba52cc-0226-4093-9c43-8c873a9c11a2"},
        };

        var e = Assert.Throws<RpcException>(() => _pagesClient.UpdatePage(updateRequest));
        Assert.Equal(StatusCode.FailedPrecondition, e.StatusCode);
    }

    [Fact]
    public void TestCreateNested()
    {
        var respCreate1 = _pagesClient.CreatePage(new()
        {
            Slug   = "akce-2021",
            SiteId = _siteId,
        });

        var respCreate2 = _pagesClient.CreatePage(new()
        {
            Slug     = "silvestr",
            SiteId   = _siteId,
            ParentId = respCreate1.Id,
        });

        var respGet = _pagesClient.GetPage(new() {PageId = respCreate2.Id});

        Assert.Equal(respCreate2.Id, respGet.Page.Id);
        Assert.Equal(respCreate1.Id, respGet.Page.ParentId);
    }

    [Fact]
    public void TestUpdate()
    {
        var respCreate = _pagesClient.CreatePage(new()
        {
            Name    = "Silvestr 2021",
            Slug    = "silvestr-2021",
            Content = "Sešli jsme se...",
            SiteId  = _siteId,
            Role    = "event",
        });

        var id = respCreate.Id;

        var respGet1 = _pagesClient.GetPage(new() {PageId = id});

        _pagesClient.UpdatePage(new()
        {
            PageId  = id,
            Name    = "Podzimky 2021",
            Slug    = "podzimky-2021",
            Content = "Sešli jsme se na nádraží...",
            Role    = "action",
        });

        var respGet2 = _pagesClient.GetPage(new() {PageId = id});

        Assert.Equal(id, respGet2.Page.Id);
        Assert.Equal("Podzimky 2021", respGet2.Page.Name);
        Assert.Equal("podzimky-2021", respGet2.Page.Slug);
        Assert.Equal("Sešli jsme se na nádraží...", respGet2.Page.Content);
        Assert.Equal("action", respGet2.Page.Role);
        Assert.True(respGet2.Page.UpdatedAt > respGet1.Page.UpdatedAt);
    }

    [Fact]
    public void TestDelete()
    {
        var respCreate = _pagesClient.CreatePage(new()
        {
            Slug   = "silvestr-2021",
            SiteId = _siteId,
        });

        _pagesClient.DeletePage(new() {PageId = respCreate.Id});

        var e = Assert.Throws<RpcException>(() => _pagesClient.GetPage(new() {PageId = respCreate.Id}));
        Assert.Equal(StatusCode.NotFound, e.StatusCode);
    }

    [Fact]
    public void TestGetBySiteAndPath()
    {
        var respCreate = _pagesClient.CreatePage(new()
        {
            Slug   = "silvestr-2021",
            SiteId = _siteId,
        });

        var respGet = _pagesClient.GetPageBySiteAndPath(new() {SiteId = _siteId, Path = "silvestr-2021"});

        Assert.Equal(respCreate.Id, respGet.Page.Id);
    }

    [Fact]
    public void TestGetBySiteAndPathNested()
    {
        var respCreate1 = _pagesClient.CreatePage(new()
        {
            Slug   = "vypravy",
            SiteId = _siteId,
        });

        var respCreate2 = _pagesClient.CreatePage(new()
        {
            Slug     = "silvestr-2021",
            SiteId   = _siteId,
            ParentId = respCreate1.Id,
        });

        var respGet = _pagesClient.GetPageBySiteAndPath(new() {SiteId = _siteId, Path = "vypravy/silvestr-2021"});

        Assert.Equal(respCreate2.Id, respGet.Page.Id);
    }

    [Fact]
    public void TestGetBySiteAndPathWithEventSlug()
    {
        var respCreateEventsPage = _pagesClient.CreatePage(new()
        {
            Slug   = "akce",
            SiteId = _siteId,
        });
        var respCreateEventDetailPage = _pagesClient.CreatePage(new()
        {
            Slug     = "oddil-{event:slug}",
            SiteId   = _siteId,
            ParentId = respCreateEventsPage.Id,
        });
        _eventsClient.CreateEvent(new()
        {
            Slug = "silvestr-2021",
        });

        var respGet = _pagesClient.GetPageBySiteAndPath(new() {SiteId = _siteId, Path = "akce/oddil-silvestr-2021"});

        Assert.Equal(respCreateEventDetailPage.Id, respGet.Page.Id);
    }

    [Fact]
    public void TestGetBySiteAndPathFailsWhenPageDoesNotExist()
    {
        var request = new GetPageBySiteAndPathRequest {SiteId = _siteId, Path = "silvestr-2021"};

        var e = Assert.Throws<RpcException>(() => _pagesClient.GetPageBySiteAndPath(request));
        Assert.Equal(StatusCode.NotFound, e.StatusCode);
    }

    [Fact]
    public void TestGetMany()
    {
        _pagesClient.CreatePage(new()
        {
            Name    = "Podzimky 2021",
            Slug    = "podzimky-2021",
            Content = "Sešli jsme se...",
            SiteId  = _siteId,
        });
        _pagesClient.CreatePage(new()
        {
            Name    = "Silvestr 2021",
            Slug    = "silvestr-2021",
            Content = "Opět jsme se sešli...",
            SiteId  = _siteId,
        });
        _pagesClient.CreatePage(new()
        {
            Name    = "Brdy 2022",
            Slug    = "brdy-2022",
            Content = "Byla zima.",
            SiteId  = _siteId,
        });

        var resp = _pagesClient.GetPages(new());

        Assert.Equal(3, resp.Pages.Count);
        Assert.All(resp.Pages, p => Assert.NotNull(p.Id));
        Assert.All(resp.Pages, p => Assert.Equal(_siteId, p.SiteId));
        Assert.Contains(resp.Pages, p => p.Name == "Podzimky 2021");
        Assert.Contains(resp.Pages, p => p.Slug == "podzimky-2021");
        Assert.Contains(resp.Pages, p => p.Content == "Sešli jsme se...");
        Assert.Contains(resp.Pages, p => p.Name == "Silvestr 2021");
        Assert.Contains(resp.Pages, p => p.Slug == "silvestr-2021");
        Assert.Contains(resp.Pages, p => p.Content == "Opět jsme se sešli...");
        Assert.Contains(resp.Pages, p => p.Name == "Brdy 2022");
        Assert.Contains(resp.Pages, p => p.Slug == "brdy-2022");
        Assert.Contains(resp.Pages, p => p.Content == "Byla zima.");
    }
}
