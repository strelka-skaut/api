using Api.Data;
using ApiSpec.Grpc.Events;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace E2eTest;

public class EventTest : IClassFixture<Fixture>, IDisposable
{
    private readonly MainDb                _db;
    private readonly Service.ServiceClient _client;

    public EventTest(Fixture fixture)
    {
        _db     = fixture.Db;
        _client = new Service.ServiceClient(fixture.Channel);
    }

    public void Dispose()
    {
        _db.Database.ExecuteSqlRaw("DELETE FROM \"Event\";");
    }

    [Fact]
    public void TestCreateAndGet()
    {
        var respCreate = _client.CreateEvent(new()
        {
            Name = "Silvestr 2021",
            Slug = "silvestr-2021",
        });

        Assert.NotNull(respCreate.Id);

        var respGet = _client.GetEvent(new() {EventId = respCreate.Id});

        Assert.Equal(respCreate.Id, respGet.Event.Id);
        Assert.Equal("Silvestr 2021", respGet.Event.Name);
        Assert.Equal("silvestr-2021", respGet.Event.Slug);
    }

    [Fact]
    public void TestUpdate()
    {
        var respCreate = _client.CreateEvent(new()
        {
            Name = "Silvestr 2021",
            Slug = "silvestr-2021",
        });

        var id = respCreate.Id;

        _client.UpdateEvent(new()
        {
            EventId = id,
            Name    = "Podzimky 2021",
            Slug    = "podzimky-2021",
        });

        var respGet2 = _client.GetEvent(new() {EventId = id});

        Assert.Equal(id, respGet2.Event.Id);
        Assert.Equal("Podzimky 2021", respGet2.Event.Name);
        Assert.Equal("podzimky-2021", respGet2.Event.Slug);
    }

    [Fact]
    public void TestDelete()
    {
        var respCreate = _client.CreateEvent(new()
        {
            Slug = "silvestr-2021",
        });

        _client.DeleteEvent(new() {EventId = respCreate.Id});

        var e = Assert.Throws<RpcException>(() => _client.GetEvent(new() {EventId = respCreate.Id}));
        Assert.Equal(StatusCode.NotFound, e.StatusCode);
    }

    [Fact]
    public void TestGetMany()
    {
        _client.CreateEvent(new()
        {
            Name = "Silvestr 2021",
            Slug = "silvestr-2021",
        });
        _client.CreateEvent(new()
        {
            Name = "Podzimky 2021",
            Slug = "podzimky-2021",
        });
        _client.CreateEvent(new()
        {
            Name = "Velikonoce 2022",
            Slug = "velikonoce-2022",
        });

        var resp = _client.GetEvents(new());

        Assert.Equal(3, resp.Events.Count);
        Assert.All(resp.Events, p => Assert.NotNull(p.Id));
        Assert.Contains(resp.Events, p => p.Name == "Silvestr 2021");
        Assert.Contains(resp.Events, p => p.Slug == "silvestr-2021");
        Assert.Contains(resp.Events, p => p.Name == "Podzimky 2021");
        Assert.Contains(resp.Events, p => p.Slug == "podzimky-2021");
        Assert.Contains(resp.Events, p => p.Name == "Velikonoce 2022");
        Assert.Contains(resp.Events, p => p.Slug == "velikonoce-2022");
    }
}
