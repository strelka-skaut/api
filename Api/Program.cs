using System.Net;
using Api;
using Api.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PageService = Api.Services.PageService;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 2000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
});

builder.Services.AddGrpc(o => { o.Interceptors.Add<ExceptionInterceptor>(); });

builder.Services.AddDbContext<MainDb>();

var app = builder.Build();

app.UseRouting();

app.MapGrpcService<PageService>();

app.Run();
