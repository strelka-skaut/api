using Api;
using Grpc.Net.Client;
using Xunit;

namespace E2eTest;

public class PageTest
{
    [Fact]
    public void TestGetPage()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:2000");
        var client = new PageService.PageServiceClient(channel);

        var resp = client.GetPage(new PageServiceGetPageRequest
        {
            PageId = 1
        });

        Assert.Equal("Podzimky 2021", resp.Page.Title);
    }
}
