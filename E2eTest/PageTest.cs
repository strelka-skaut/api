using Api;
using Grpc.Net.Client;
using Xunit;

namespace E2eTest;

public class PageTest
{
    [Fact]
    public void TestGet()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:2000");
        var client = new PageService.PageServiceClient(channel);

        // var resp = client.GetPage(new PageServiceGetPageRequest
        // {
        // PageId = 1
        // });

        // Assert.Equal("Podzimky 2021", resp.Page.Title);
    }

    [Fact]
    public void TestCreateAndGet()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:2000");
        var client = new PageService.PageServiceClient(channel);

        var respCreate = client.CreatePage(new PageServiceCreatePageRequest()
        {
            Name = "Silvestr 2021",
            Content = "Sesli jsme se...",
        });

        var id = respCreate.Id;

        var respGet = client.GetPage(new PageServiceGetPageRequest
        {
            PageId = new Uuid {Value = id.Value},
        });

        Assert.Equal(id, respGet.Page.Id);
        Assert.Equal("Silvestr 2021", respGet.Page.Name);
        Assert.Equal("Sesli jsme se...", respGet.Page.Content);
        Assert.Null(respGet.Page.SiteId);
    }
}
