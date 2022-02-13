using ApiSpec.Grpc;
using ApiSpec.Grpc.Pages;
using Grpc.Net.Client;
using Xunit;

namespace E2eTest;

public class PageTest
{
    [Fact]
    public void TestCreateAndGetAndDelete()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:2000");
        var client = new Service.ServiceClient(channel);

        var respCreate = client.CreatePage(new CreatePageRequest
        {
            Name = "Silvestr 2021",
            Slug = "silvestr-2021",
            Content = "Sesli jsme se...",
        });

        Assert.NotNull(respCreate.Id);

        var respGet = client.GetPage(new GetPageRequest
        {
            PageId = respCreate.Id,
        });

        Assert.Equal(respCreate.Id, respGet.Page.Id);
        Assert.Equal("Silvestr 2021", respGet.Page.Name);
        Assert.Equal("silvestr-2021", respGet.Page.Slug);
        Assert.Equal("Sesli jsme se...", respGet.Page.Content);
        Assert.True(respGet.Page.UpdatedAt.ToDateTime().Year > 0);

        client.DeletePage(new DeletePageRequest
        {
            PageId = respCreate.Id
        });
    }

    [Fact]
    public void TestGetPages()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:2000");
        var client = new Service.ServiceClient(channel);

        var ids = new List<Uuid>();
        ids.Add(client.CreatePage(new CreatePageRequest
        {
            Name = "Podzimky 2021",
            Slug = "podzimky-2021",
            Content = "Sesli jsme se...",
        }).Id);
        ids.Add(client.CreatePage(new CreatePageRequest
        {
            Name = "Silvestr 2021",
            Slug = "silvestr-2021",
            Content = "Opet jsme se sesli...",
        }).Id);
        ids.Add(client.CreatePage(new CreatePageRequest
        {
            Name = "Brdy 2022",
            Slug = "brdy-2022",
            Content = "Byla zima.",
        }).Id);

        try
        {
            var resp = client.GetPages(new GetPagesRequest());

            Assert.Equal(3, resp.Pages.Count);
            Assert.All(resp.Pages, p => Assert.NotNull(p.Id));
            Assert.Contains(resp.Pages, p => p.Name == "Podzimky 2021");
            Assert.Contains(resp.Pages, p => p.Slug == "podzimky-2021");
            Assert.Contains(resp.Pages, p => p.Content == "Sesli jsme se...");
            Assert.Contains(resp.Pages, p => p.Name == "Silvestr 2021");
            Assert.Contains(resp.Pages, p => p.Slug == "silvestr-2021");
            Assert.Contains(resp.Pages, p => p.Content == "Opet jsme se sesli...");
            Assert.Contains(resp.Pages, p => p.Name == "Brdy 2022");
            Assert.Contains(resp.Pages, p => p.Slug == "brdy-2022");
            Assert.Contains(resp.Pages, p => p.Content == "Byla zima.");
        }
        finally
        {
            // todo jen truncate jednoduse
            foreach (var id in ids)
                client.DeletePage(new DeletePageRequest
                {
                    PageId = id
                });
        }
    }
}
