using Api.Data;
using ApiSpec.Grpc.Pages;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace E2eTest;

public class PageTest : IDisposable
{
    // todo ctor i Disposable to fixture
    private readonly Service.ServiceClient _client;

    public PageTest()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:2000");
        _client = new Service.ServiceClient(channel);
    }

    public void Dispose()
    {
        var builder = new DbContextOptionsBuilder<MainDb>();
        builder.UseNpgsql("server=localhost;port=5432;user id=root;password=root;database=main");

        var db = new MainDb(builder.Options);
        db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Page\";");
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
        var ids = new[]
        {
            _client.CreatePage(new()
            {
                Name    = "Podzimky 2021",
                Slug    = "podzimky-2021",
                Content = "Sešli jsme se...",
            }).Id,
            _client.CreatePage(new()
            {
                Name    = "Silvestr 2021",
                Slug    = "silvestr-2021",
                Content = "Opět jsme se sešli...",
            }).Id,
            _client.CreatePage(new()
            {
                Name    = "Brdy 2022",
                Slug    = "brdy-2022",
                Content = "Byla zima.",
            }).Id,
        };

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
