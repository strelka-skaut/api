using System.Net;
using Api;
using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 2000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
});

builder.Services.AddGrpc(o => { o.Interceptors.Add<ExceptionInterceptor>(); });

builder.Services.AddDbContext<MainDb>(options =>
{
    options.UseNpgsql(builder.Configuration["Database:NpgsqlString"]);
});

builder.Services.AddSingleton(
    _ => new GoogleApiServiceFactory(builder.Configuration["Google:ServiceAccountCredentialFile"]).CreateDriveService()
);

builder.Services.AddSingleton(_ => new MediaService.Configuration
{
    FileCacheBasePath = builder.Configuration["FileCache:BasePath"],
    FileCacheBaseUrl = builder.Configuration["FileCache:BaseUrl"],
});

var app = builder.Build();

app.UseRouting();

app.MapGrpcService<PageService>();
app.MapGrpcService<MediaService>();

app.Run();
