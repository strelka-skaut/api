using ApiSpec.Grpc.Media;
using Grpc.Core;
using Grpc.Net.Client;
using Xunit;
using Xunit.Abstractions;

namespace E2eTest;

public class MediaTest
{
    private const string GrpcServer = "http://localhost:2000";

    private readonly ITestOutputHelper _testOutputHelper;

    public MediaTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async void TestGetFileUrl()
    {
        var channel = GrpcChannel.ForAddress(GrpcServer);
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

    [Fact]
    public async void TestGetPhotoThumbnailUrls()
    {
        var channel = GrpcChannel.ForAddress(GrpcServer);
        var client = new Service.ServiceClient(channel);

        var resp = client.GetPhotoThumbnailUrls(new GetPhotoThumbnailUrlsRequest
        {
            GdriveFileIds =
            {
                "1EdHbPBd5eaw1Oi37GHCLXn12pRe0iBGV",
                "1im6i-_uBRTA2T2Jfekl-Zd2tcfIYEywc",
                "10ZHdnZRYqCgQ2VzOiwmdz2HvYZ5dFcrX",
                "1zNkNDVSqfX6M9iyMJLXHzXsPDYup7YWL"
            },
            Width = 250,
        });

        Assert.Equal(4, resp.Urls.Count);

        foreach (var url in resp.Urls)
            _testOutputHelper.WriteLine(url);

        var http = new HttpClient();

        foreach (var url in resp.Urls)
        {
            var httpResp = await http.GetAsync(url);
            Assert.True(httpResp.IsSuccessStatusCode);
        }
    }

    [Fact]
    public async void TestCreateGallery()
    {
        var channel = GrpcChannel.ForAddress(GrpcServer);
        var client = new Service.ServiceClient(channel);

        var respCreate = client.CreateGallery(new CreateGalleryRequest
        {
            Name = "Moje krásná galerie. Nešahat!",
            Slug = "moje-krasna-galerie-nesahat",
            GdriveFolderId = "1LGv0DUazad93AIZ6T5SyqonkCA9BV4hp",
        });

        Assert.NotNull(respCreate.Id);

        var galleryId = respCreate.Id;

        var respGet = client.GetGallery(new GetGalleryRequest
        {
            GalleryId = galleryId,
        });

        Assert.Equal(galleryId, respGet.Gallery.Id);
        Assert.Equal("Moje krásná galerie. Nešahat!", respGet.Gallery.Name);
        Assert.Equal("moje-krasna-galerie-nesahat", respGet.Gallery.Slug);
        Assert.Equal("1LGv0DUazad93AIZ6T5SyqonkCA9BV4hp", respGet.Gallery.GdriveFolderId);
        Assert.Null(respGet.Gallery.SiteId);

        client.DeleteGallery(new DeleteGalleryRequest
        {
            GalleryId = galleryId,
        });

        var e = Assert.Throws<RpcException>(() =>
        {
            client.GetGallery(new GetGalleryRequest
            {
                GalleryId = galleryId,
            });
        });
        Assert.Equal(StatusCode.NotFound, e.StatusCode);
    }
}
