using System.Net;
using Api;
using Api.Data;
using Api.Services;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 2000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
});

builder.Services.AddGrpc(o => { o.Interceptors.Add<ExceptionInterceptor>(); });

builder.Services.AddDbContext<MainDb>();
builder.Services.Add(new ServiceDescriptor(typeof(DriveService), new DriveServiceFactory().Create()));

var app = builder.Build();

app.UseRouting();

app.MapGrpcService<PageService>();
app.MapGrpcService<MediaService>();

app.Run();
