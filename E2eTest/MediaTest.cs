using ApiSpec.Grpc.Media;
using Grpc.Net.Client;
using Xunit;
using Xunit.Abstractions;

namespace E2eTest;

public class MediaTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MediaTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async void TestGetFileUrl()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:2000");
        var client = new Service.ServiceClient(channel);

        var resp = client.GetFileUrl(new GetFileUrlRequest
        {
            GdriveFileId = "1EdHbPBd5eaw1Oi37GHCLXn12pRe0iBGV"
        });

        Assert.NotEqual("", resp.Url);

        _testOutputHelper.WriteLine(resp.Url);

        var http = new HttpClient();
        var httpResp = await http.GetAsync(resp.Url);

        Assert.True(httpResp.IsSuccessStatusCode);
    }
}
