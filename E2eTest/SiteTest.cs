using Api.Data;
using ApiSpec.Grpc.Sites;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace E2eTest;

public class SiteTest : IClassFixture<Fixture>, IDisposable
{
    private readonly MainDb                _db;
    private readonly Service.ServiceClient _client;

    public SiteTest(Fixture fixture)
    {
        _db     = fixture.Db;
        _client = new Service.ServiceClient(fixture.Channel);
    }

    public void Dispose()
    {
        _db.Database.ExecuteSqlRaw("DELETE FROM \"Site\";");
    }

    [Fact]
    public void TestCreateAndGet()
    {
        var respCreate = _client.CreateSite(new()
        {
            Name = "Stopaři",
            Slug = "stopari",
        });

        Assert.NotNull(respCreate.Id);

        var respGet = _client.GetSite(new() {SiteId = respCreate.Id});

        Assert.Equal(respCreate.Id, respGet.Site.Id);
        Assert.Equal("Stopaři", respGet.Site.Name);
        Assert.Equal("stopari", respGet.Site.Slug);
    }

    [Fact]
    public void TestUpdate()
    {
        var respCreate = _client.CreateSite(new()
        {
            Name = "Stopaři",
            Slug = "stopari",
        });

        var id = respCreate.Id;

        _client.UpdateSite(new()
        {
            SiteId = id,
            Name   = "Kvítka",
            Slug   = "kvitka",
        });

        var respGet2 = _client.GetSite(new() {SiteId = id});

        Assert.Equal(id, respGet2.Site.Id);
        Assert.Equal("Kvítka", respGet2.Site.Name);
        Assert.Equal("kvitka", respGet2.Site.Slug);
    }

    [Fact]
    public void TestDelete()
    {
        var respCreate = _client.CreateSite(new()
        {
            Slug = "stopari",
        });

        _client.DeleteSite(new() {SiteId = respCreate.Id});

        var e = Assert.Throws<RpcException>(() => _client.GetSite(new() {SiteId = respCreate.Id}));
        Assert.Equal(StatusCode.NotFound, e.StatusCode);
    }

    [Fact]
    public void TestGetBySlug()
    {
        var respCreate = _client.CreateSite(new()
        {
            Slug = "stopari",
        });

        var respGet = _client.GetSiteBySlug(new() {SiteSlug = "stopari"});

        Assert.Equal(respCreate.Id, respGet.Site.Id);
    }

    [Fact]
    public void TestGetBySlugFailsWhenSiteDoesNotExist()
    {
        var e = Assert.Throws<RpcException>(() => _client.GetSiteBySlug(new() {SiteSlug = "stopari"}));
        Assert.Equal(StatusCode.NotFound, e.StatusCode);
    }

    [Fact]
    public void TestGetMany()
    {
        _client.CreateSite(new()
        {
            Name = "Stopaři",
            Slug = "stopari",
        });
        _client.CreateSite(new()
        {
            Name = "Kvítka",
            Slug = "kvitka",
        });
        _client.CreateSite(new()
        {
            Name = "Vlčata",
            Slug = "vlcata",
        });

        var resp = _client.GetSites(new());

        Assert.Equal(3, resp.Sites.Count);
        Assert.All(resp.Sites, p => Assert.NotNull(p.Id));
        Assert.Contains(resp.Sites, p => p.Name == "Stopaři");
        Assert.Contains(resp.Sites, p => p.Slug == "stopari");
        Assert.Contains(resp.Sites, p => p.Name == "Kvítka");
        Assert.Contains(resp.Sites, p => p.Slug == "kvitka");
        Assert.Contains(resp.Sites, p => p.Name == "Vlčata");
        Assert.Contains(resp.Sites, p => p.Slug == "vlcata");
    }
}
