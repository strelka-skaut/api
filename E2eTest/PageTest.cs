using Api.Data;
using ApiSpec.Grpc.Pages;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace E2eTest;

public class PageTest : IClassFixture<Fixture>, IDisposable
{
    private readonly MainDb                _db;
    private readonly Service.ServiceClient _client;

    public PageTest(Fixture fixture)
    {
        _db     = fixture.Db;
        _client = new Service.ServiceClient(fixture.Channel);
    }

    public void Dispose()
    {
        _db.Database.ExecuteSqlRaw("DELETE FROM \"Page\";");
    }

    [Fact]
    public void TestCreateAndGet()
    {
        var respCreate = _client.CreatePage(new()
        {
            Name    = "Silvestr 2021",
            Slug    = "silvestr-2021",
            Content = "Sešli jsme se...",
        });

        Assert.NotNull(respCreate.Id);

        var respGet = _client.GetPage(new() {PageId = respCreate.Id});

        Assert.Equal(respCreate.Id, respGet.Page.Id);
        Assert.Equal("Silvestr 2021", respGet.Page.Name);
        Assert.Equal("silvestr-2021", respGet.Page.Slug);
        Assert.Equal("Sešli jsme se...", respGet.Page.Content);
        Assert.True(respGet.Page.UpdatedAt.ToDateTime().Year > 0);
        Assert.Null(respGet.Page.ParentId);
    }

    [Fact]
    public void TestCreateNested()
    {
        var respCreate1 = _client.CreatePage(new()
        {
            Name = "Akce 2021",
            Slug = "akce-2021",
        });

        var respCreate2 = _client.CreatePage(new()
        {
            Name     = "Silvestr",
            Slug     = "silvestr",
            Content  = "Sešli jsme se...",
            ParentId = respCreate1.Id,
        });

        var respGet = _client.GetPage(new() {PageId = respCreate2.Id});

        Assert.Equal(respCreate2.Id, respGet.Page.Id);
        Assert.Equal(respCreate1.Id, respGet.Page.ParentId);
    }

    [Fact]
    public void TestUpdate()
    {
        var respCreate = _client.CreatePage(new()
        {
            Name    = "Silvestr 2021",
            Slug    = "silvestr-2021",
            Content = "Sešli jsme se...",
        });

        var id = respCreate.Id;

        var respGet1 = _client.GetPage(new() {PageId = id});

        _client.UpdatePage(new()
        {
            PageId  = id,
            Name    = "Podzimky 2021",
            Slug    = "podzimky-2021",
            Content = "Sešli jsme se na nádraží...",
        });

        var respGet2 = _client.GetPage(new() {PageId = id});

        Assert.Equal(id, respGet2.Page.Id);
        Assert.Equal("Podzimky 2021", respGet2.Page.Name);
        Assert.Equal("podzimky-2021", respGet2.Page.Slug);
        Assert.Equal("Sešli jsme se na nádraží...", respGet2.Page.Content);
        Assert.True(respGet2.Page.UpdatedAt > respGet1.Page.UpdatedAt);
    }

    [Fact]
    public void TestDelete()
    {
        var respCreate = _client.CreatePage(new()
        {
            Name    = "Silvestr 2021",
            Slug    = "silvestr-2021",
            Content = "Sešli jsme se...",
        });

        _client.DeletePage(new() {PageId = respCreate.Id});

        var e = Assert.Throws<RpcException>(() => _client.GetPage(new() {PageId = respCreate.Id}));
        Assert.Equal(StatusCode.NotFound, e.StatusCode);
    }

    [Fact]
    public void TestGetBySlug()
    {
        var respCreate = _client.CreatePage(new()
        {
            Name    = "Silvestr 2021",
            Slug    = "silvestr-2021",
            Content = "Sešli jsme se...",
        });

        var respGet = _client.GetPageBySlug(new() {PageSlug = "silvestr-2021"});

        Assert.Equal(respCreate.Id, respGet.Page.Id);
    }

    [Fact]
    public void TestGetMany()
    {
        _client.CreatePage(new()
        {
            Name    = "Podzimky 2021",
            Slug    = "podzimky-2021",
            Content = "Sešli jsme se...",
        });
        _client.CreatePage(new()
        {
            Name    = "Silvestr 2021",
            Slug    = "silvestr-2021",
            Content = "Opět jsme se sešli...",
        });
        _client.CreatePage(new()
        {
            Name    = "Brdy 2022",
            Slug    = "brdy-2022",
            Content = "Byla zima.",
        });

        var resp = _client.GetPages(new());

        Assert.Equal(3, resp.Pages.Count);
        Assert.All(resp.Pages, p => Assert.NotNull(p.Id));
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
