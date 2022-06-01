using System.Net;
using System.Security.Cryptography;
using Api;
using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // options.TokenValidationParameters.ValidIssuer = "https://auth.dev.strelka.cz/auth/realms/strelka";
        // options.TokenValidationParameters.IssuerSigningKey = new JsonWebKey(
        // "{\"keys\":[{\"kid\":\"4onUez3W03_h1YkhE0PWzR8J_n-9WGJDPlG8gok4K9k\",\"kty\":\"RSA\",\"alg\":\"RSA-OAEP\",\"use\":\"enc\",\"n\":\"t5vrAQSPjzT146hstPPVtQNjhmhf9PkfU2Zhf6AIEkMiLDrYH55ORgSt37q3AK9oBvHyXrF_picA35lNVtIH9Swobs42D9wIJBV_oXky9OCOTPU3JlVBdrzgFKsAmz9xTuHRNIfVJpalExkgPypFyAwpmNmH7XpTMYuwehfYUEw89OgJcV144YQCjupEWSJ2HaKKv-21uSHYuH8fhg8tbo_wJ1P_-Lfo8x8ycuLq6kvJUvtNLCwu1MXUonL0pcI3IwLGf13IXsAnthcyhujqRBiD7CfrWWAEgF4BxNmN3vmRycyUiEw-XwvuBjrRggnrwTD2-bzk9-ejzrzJX5xyRw\",\"e\":\"AQAB\",\"x5c\":[\"MIICnTCCAYUCBgGACX/0EjANBgkqhkiG9w0BAQsFADASMRAwDgYDVQQDDAdzdHJlbGthMB4XDTIyMDQwODE0MDUyMloXDTMyMDQwODE0MDcwMlowEjEQMA4GA1UEAwwHc3RyZWxrYTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALeb6wEEj4809eOobLTz1bUDY4ZoX/T5H1NmYX+gCBJDIiw62B+eTkYErd+6twCvaAbx8l6xf6YnAN+ZTVbSB/UsKG7ONg/cCCQVf6F5MvTgjkz1NyZVQXa84BSrAJs/cU7h0TSH1SaWpRMZID8qRcgMKZjZh+16UzGLsHoX2FBMPPToCXFdeOGEAo7qRFkidh2iir/ttbkh2Lh/H4YPLW6P8CdT//i36PMfMnLi6upLyVL7TSwsLtTF1KJy9KXCNyMCxn9dyF7AJ7YXMobo6kQYg+wn61lgBIBeAcTZjd75kcnMlIhMPl8L7gY60YIJ68Ew9vm85Pfno868yV+cckcCAwEAATANBgkqhkiG9w0BAQsFAAOCAQEAsW/yzkOaIac5YHDikaaMQ7f8Ps31WPvANayk5NEfEok4U1JZ6a7NckCJUhIx/aGQ4APcMYhmAnJT1HRy6KmAqXIWYejraBfeDfS1LAd0Lco7GddFgZnJf9tVhcrE310v2oKYXy9OU0Z+jXepW88UgmmaTaciGYg7IN+Dy7bG9YiDBEgNdO6TdxXA0nyUrcKm5p95fpgfjDvK4w7AInfwAcRNiUQald96YHmyLGsXu44DKWsJ0FDO3WC8pZeRHi6RcupWqEXVmIX2XszNo/qNs7rkMwcntvO1XpZ6yxU8lhUAUhDt0wC8lk/VSBxogtw1FsPiqNN1TBfyItNL8wU//A==\"],\"x5t\":\"hLpbX-qGMB-p5BPPD8fpflkZYws\",\"x5t#S256\":\"Ub4MRrbsMvcNciOoudnUbP7NcJloHrAzdgVQP6K8vLQ\"},{\"kid\":\"cvpwibK7kor6dgcral8y-yexOG6aYDvTvxq7rEiVUu8\",\"kty\":\"RSA\",\"alg\":\"RS256\",\"use\":\"sig\",\"n\":\"3IV1npB7pXY0G0CIae4SeiJkq-iq1QsJ5ryarMkbL_Trdg-W9P1-jYUh_dR0CHyS2-1oj0XFD_4aUz41jFMYWWIDrQaYNcEgTWfLB5fA0-P39mbmVaw5dRDXEE3lJ_1-ObAd5y_1syGT_YPGw8CQT20h84BAtXUYESm34uGHT_s9Me_nROTz6GbIXGU7ehH4g8jYnmjwOOFPeA7jeXL3nqw09FliYpoItWpkDe2-jfLe0gkGPXPi75Xd4BMlLs5rUCz_yMCYdE1wtWZycPh1DHTjG6yK2Tx2Hbtkwid7QkxF_8J41FxTUnXhImo0rlpXtyJaar_BrKFKrEbhqg2TAw\",\"e\":\"AQAB\",\"x5c\":[\"MIICnTCCAYUCBgGACX/wDTANBgkqhkiG9w0BAQsFADASMRAwDgYDVQQDDAdzdHJlbGthMB4XDTIyMDQwODE0MDUyMVoXDTMyMDQwODE0MDcwMVowEjEQMA4GA1UEAwwHc3RyZWxrYTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBANyFdZ6Qe6V2NBtAiGnuEnoiZKvoqtULCea8mqzJGy/063YPlvT9fo2FIf3UdAh8ktvtaI9FxQ/+GlM+NYxTGFliA60GmDXBIE1nyweXwNPj9/Zm5lWsOXUQ1xBN5Sf9fjmwHecv9bMhk/2DxsPAkE9tIfOAQLV1GBEpt+Lhh0/7PTHv50Tk8+hmyFxlO3oR+IPI2J5o8DjhT3gO43ly956sNPRZYmKaCLVqZA3tvo3y3tIJBj1z4u+V3eATJS7Oa1As/8jAmHRNcLVmcnD4dQx04xusitk8dh27ZMIne0JMRf/CeNRcU1J14SJqNK5aV7ciWmq/wayhSqxG4aoNkwMCAwEAATANBgkqhkiG9w0BAQsFAAOCAQEAZZMVcpi/LpO5A3EDAW/X6msa30v2HoF0zG6KQ1Lo2z2v6o9hpcDBOaM9W9J6YE6H6a6S0xpTctKPaEON8yeXlom8FH0viZgrs3O32JVWCkVCJoH3dUIzQpOOw/pQbGHX1z+lt5ByxCkcp7Tc3EXT5QC3WmVCl6dg893KCKwQk0IqFr5zT/aYi/jbLIc1xpdNEWufhHyV3mF/6qz/ZIRfPBqC5u8DyYhfevxK5Kw8rKxhNSL8MWz9no+LLPpbWenq0aapOKIbmsMnTS/5qjGX9QvG1+X5i2+TMlJsstBVmwyVHs7dDBOlyKhT8ZPjTWCPVf3IMslwEhu33jgjPkMH9w==\"],\"x5t\":\"E07FBzR7PmM-zFrh9t2b4H7HaA8\",\"x5t#S256\":\"XWtJFvfha0BduvnTpDQRL5x6J6PhWE5JjWZJJS_TwfY\"}]}");
        options.Authority = "https://auth.dev.strelka.cz/auth/realms/strelka";
        options.Audience  = "account";
        // options.TokenValidationParameters
        // options.RequireHttpsMetadata                       = false;
        // options.TokenValidationParameters.ValidateAudience = false;
        // options.TokenValidationParameters.ValidateIssuer = true;
        // options.TokenValidationParameters.ValidateIssuerSigningKey = true;
    });

builder.Services.AddSingleton(_ => new MediaService.Configuration
{
    FileCacheBasePath = builder.Configuration["FileCache:BasePath"],
    FileCacheBaseUrl  = builder.Configuration["FileCache:BaseUrl"],
});

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
// app.UseAuthorization();

app.MapGrpcService<MediaService>();
app.MapGrpcService<PageService>();
app.MapGrpcService<SiteService>();

app.Run();
