using Api.Data;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;

namespace E2eTest;

public class Fixture : IDisposable
{
    public GrpcChannel Channel { get; }
    public MainDb      Db      { get; }

    public Fixture()
    {
        Channel = GrpcChannel.ForAddress("http://localhost:2000");

        var builder = new DbContextOptionsBuilder<MainDb>();
        builder.UseNpgsql("server=localhost;port=5432;user id=root;password=root;database=main");

        Db = new MainDb(builder.Options);
    }

    public void Dispose()
    {
        Channel.Dispose();
        Db.Dispose();
    }
}
