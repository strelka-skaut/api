using Api.Data;
using ApiSpec.Grpc.Media;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace E2eTest;

public class MediaTest : IClassFixture<Fixture>, IDisposable
{
    private readonly ITestOutputHelper     _output;
    private readonly MainDb                _db;
    private readonly Service.ServiceClient _client;

    public MediaTest(Fixture fixture, ITestOutputHelper output)
    {
        _output = output;
        _db     = fixture.Db;
        _client = new Service.ServiceClient(fixture.Channel);
    }

    public void Dispose()
    {
        _db.Database.ExecuteSqlRaw("DELETE FROM \"Photo\";");
        _db.Database.ExecuteSqlRaw("DELETE FROM \"Gallery\";");
    }

    [Fact]
    public async void TestGetFileUrl()
    {
        var resp = _client.GetFileUrl(new() {GdriveFileId = "1EdHbPBd5eaw1Oi37GHCLXn12pRe0iBGV"});

        Assert.NotEqual("", resp.Url);

        _output.WriteLine(resp.Url);

        var http = new HttpClient();
        var httpResp = await http.GetAsync(resp.Url);

        Assert.True(httpResp.IsSuccessStatusCode);
    }

    [Fact]
    public async void TestGetPhotoThumbnailUrls()
    {
        var resp = _client.GetPhotoThumbnailUrls(new()
        {
            GdriveFileIds =
            {
                "1EdHbPBd5eaw1Oi37GHCLXn12pRe0iBGV",
                "1im6i-_uBRTA2T2Jfekl-Zd2tcfIYEywc",
                "10ZHdnZRYqCgQ2VzOiwmdz2HvYZ5dFcrX",
                "1zNkNDVSqfX6M9iyMJLXHzXsPDYup7YWL",
            },
            Width = 250,
        });

        Assert.Equal(4, resp.Urls.Count);

        foreach (var url in resp.Urls)
            _output.WriteLine(url);

        var http = new HttpClient();

        foreach (var url in resp.Urls)
        {
            var httpResp = await http.GetAsync(url);
            Assert.True(httpResp.IsSuccessStatusCode);
        }
    }

    [Fact]
    public void TestCreateAndGetGallery()
    {
        var respCreate = _client.CreateGallery(new()
        {
            Name           = "Moje krásná galerie. Nešahat!",
            Slug           = "moje-krasna-galerie-nesahat",
            GdriveFolderId = "1LGv0DUazad93AIZ6T5SyqonkCA9BV4hp",
        });

        Assert.NotNull(respCreate.Id);

        var respGet = _client.GetGallery(new() {GalleryId = respCreate.Id});

        Assert.Equal(respCreate.Id, respGet.Gallery.Id);
        Assert.Equal("Moje krásná galerie. Nešahat!", respGet.Gallery.Name);
        Assert.Equal("moje-krasna-galerie-nesahat", respGet.Gallery.Slug);
        Assert.Equal("1LGv0DUazad93AIZ6T5SyqonkCA9BV4hp", respGet.Gallery.GdriveFolderId);
        Assert.Null(respGet.Gallery.SiteId);
    }

    [Fact]
    public void TestDeleteGallery()
    {
        var respCreate = _client.CreateGallery(new()
        {
            Name           = "Moje krásná galerie. Nešahat!",
            Slug           = "moje-krasna-galerie-nesahat",
            GdriveFolderId = "1LGv0DUazad93AIZ6T5SyqonkCA9BV4hp",
        });

        _client.DeleteGallery(new() {GalleryId = respCreate.Id});

        var e = Assert.Throws<RpcException>(() => _client.GetGallery(new() {GalleryId = respCreate.Id}));
        Assert.Equal(StatusCode.NotFound, e.StatusCode);
    }
}
