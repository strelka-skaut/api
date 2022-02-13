using System.Net;
using Api;
using Api.Data;
using Api.Services;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 2000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
});

builder.Services.AddGrpc(o => { o.Interceptors.Add<ExceptionInterceptor>(); });

builder.Services.AddDbContext<MainDb>((optionsBuilder) =>
{
    optionsBuilder.UseNpgsql(builder.Configuration["Database:NpgsqlString"]);
});

var googleApiServiceFactory = new GoogleApiServiceFactory(builder.Configuration["Google:ServiceAccountCredentialFile"]);
builder.Services.Add(new ServiceDescriptor(typeof(DriveService), googleApiServiceFactory.CreateDriveService()));

var app = builder.Build();

app.UseRouting();

app.MapGrpcService<PageService>();
app.MapGrpcService<MediaService>();

app.Run();
