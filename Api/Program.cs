using System.Net;
using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 2000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
});

builder.Services.AddGrpc();

builder.Services.AddDbContext<MainDb>();

var app = builder.Build();

app.UseRouting();

app.MapGrpcService<PageService>();

app.Run();
