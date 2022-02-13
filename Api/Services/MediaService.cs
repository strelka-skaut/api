using System.Net;
using System.Text.RegularExpressions;
using ApiSpec.Grpc;
using ApiSpec.Grpc.Media;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class MediaService : Service.ServiceBase
{
    private static readonly Regex GdriveIdRegex = new Regex(@"^[a-zA-Z0-9_-]+$");

    private Data.MainDb _db;
    private readonly ILogger<MediaService> _log;
    private readonly DriveService _driveService;
    private readonly Configuration _configuration;

    public class Configuration
    {
        public string FileCacheBasePath { get; init; } = null!;
        public string FileCacheBaseUrl { get; init; } = null!;
    }

    public MediaService(
        Data.MainDb db,
        ILogger<MediaService> log,
        DriveService driveService,
        Configuration configuration
    )
    {
        _db = db;
        _log = log;
        _driveService = driveService;
        _configuration = configuration;
    }

    private async Task<Data.Site> GetSiteById(Uuid uuid)
    {
        var id = Guid.Parse(uuid.Value);

        var dbSite = await _db.Sites.FindAsync(id);
        if (dbSite == null)
            throw new NotFound($"Site {id} not found.");

        return dbSite;
    }

    private async Task<Data.Gallery> GetGalleryById(Uuid uuid)
    {
        var id = Guid.Parse(uuid.Value);

        var dbGallery = await _db.Galleries.FindAsync(id);
        if (dbGallery == null)
            throw new NotFound($"Gallery {id} not found.");

        return dbGallery;
    }

    public override async Task<CreateGalleryResponse> CreateGallery(CreateGalleryRequest request,
        ServerCallContext context)
    {
        Guid? siteId = request.SiteId != null ? (await GetSiteById(request.SiteId)).Id : null;

        if (!GdriveIdRegex.IsMatch(request.GdriveFolderId))
            throw new Exception("Invalid GdriveFolderId");

        var id = Guid.NewGuid();
        _db.Galleries.Add(new Data.Gallery
        {
            Id = id,
            Name = request.Name,
            Slug = request.Slug,
            GdriveFolderId = request.GdriveFolderId,
            SiteId = siteId
        });

        await _db.SaveChangesAsync();

        return new CreateGalleryResponse()
        {
            Id = id.ToUuid()
        };
    }

    public override async Task<GetGalleryResponse> GetGallery(GetGalleryRequest request, ServerCallContext context)
    {
        var gallery = await GetGalleryById(request.GalleryId);

        return new GetGalleryResponse()
        {
            Gallery = new Gallery
            {
                Id = gallery.Id.ToUuid(),
                GdriveFolderId = gallery.GdriveFolderId,
                Name = gallery.Name,
                SiteId = gallery.SiteId?.ToUuid(),
                Slug = gallery.Slug
            }
        };
    }

    public override async Task<GetGalleriesResponse> GetGalleries(GetGalleriesRequest request, ServerCallContext context)
    {
        if (request.Limit > 1000)
            throw new Exception("Limit cannot be greater than 1000.");

        var dbGalleries = await _db.Galleries
            .Skip(request.HasOffset ? request.Offset : 0)
            .Take(request.HasLimit ? request.Limit : 25)
            .ToListAsync();

        var res = new GetGalleriesResponse();
        foreach (var gallery in dbGalleries)
        {
            res.Galleries.Add(new Gallery
            {
                Id = gallery.Id.ToUuid(),
                Name = gallery.Name,
                Slug = gallery.Slug,
                GdriveFolderId = gallery.GdriveFolderId,
                SiteId = gallery.SiteId?.ToUuid()
            });
        }

        return res;
    }

    public override async Task<DeleteGalleryResponse> DeleteGallery(DeleteGalleryRequest request, ServerCallContext context)
    {
        var gallery = await GetGalleryById(request.GalleryId);

        _db.Remove(gallery);
        await _db.SaveChangesAsync();

        return new DeleteGalleryResponse();
    }

    public override async Task<GetGalleryPhotosResponse> GetGalleryPhotos(GetGalleryPhotosRequest request,
        ServerCallContext context)
    {
        var dbGallery = await GetGalleryById(request.GalleryId);
        var dbGalleryPhotos = await _db.Photos.Where(x => x.GalleryId == dbGallery.Id).ToListAsync();

        return new GetGalleryPhotosResponse()
        {
            Photos =
            {
                dbGalleryPhotos.Select(p => new Gallery.Types.Photo
                {
                    Broken = p.Broken,
                    Caption = p.Caption,
                    GdriveFileId = p.GdriveFileId,
                    Id = p.Id.ToUuid(),
                    Order = p.Order
                })
            }
        };
    }

    public override async Task<SyncGalleryPhotosResponse> SyncGalleryPhotos(SyncGalleryPhotosRequest request,
        ServerCallContext context)
    {
        var dbGallery = await GetGalleryById(request.GalleryId);

        var gdriveReq = _driveService.Files.List();
        gdriveReq.Q = $"'{dbGallery.GdriveFolderId}' in parents";
        gdriveReq.SupportsAllDrives = true;
        var gdriveRes = await gdriveReq.ExecuteAsStreamAsync();




            //var req = _driveService.Files.List();


        return await base.SyncGalleryPhotos(request, context);
    }

    private async Task DownloadFile(string remoteUrl, string localPath)
    {
        using var client = new HttpClient();
        using var fileRes = await client.GetAsync(remoteUrl);
        if (fileRes.StatusCode != HttpStatusCode.OK)
            throw new Exception("DownloadFile not OK");
        await using var fileOutputStream = new FileStream(localPath, FileMode.CreateNew);
        await fileRes.Content.CopyToAsync(fileOutputStream);
    }

    public override async Task<GetFileUrlResponse> GetFileUrl(GetFileUrlRequest request, ServerCallContext context)
    {
        if (!GdriveIdRegex.IsMatch(request.GdriveFileId))
            throw new Exception("Invalid GdriveFileId");

        var cacheFile = _configuration.FileCacheBasePath + "/" + request.GdriveFileId;
        if (!File.Exists(cacheFile))
        {
            var gdriveReq = _driveService.Files.Get(request.GdriveFileId);
            gdriveReq.SupportsAllDrives = true;
            gdriveReq.Fields = "id,webContentLink";
            var gdriveRes = await gdriveReq.ExecuteAsync();
            await DownloadFile(gdriveRes.WebContentLink, cacheFile);
        }

        return new GetFileUrlResponse
        {
            Url = _configuration.FileCacheBaseUrl + "/" + request.GdriveFileId
        };
    }

    public override async Task<GetPhotoThumbnailUrlsResponse> GetPhotoThumbnailUrls(
        GetPhotoThumbnailUrlsRequest request, ServerCallContext context)
    {
        var sizeSuffix = request.MaxSizeCase switch
        {
            GetPhotoThumbnailUrlsRequest.MaxSizeOneofCase.Height => $"h{Math.Min(2500, request.Height)}",
            GetPhotoThumbnailUrlsRequest.MaxSizeOneofCase.Width => $"w{Math.Min(2500, request.Width)}",
            _ => throw new Exception("Invalid MaxSize"),
        };

        var tasks = request.GdriveFileIds.Select(async fileId =>
        {
            if (!GdriveIdRegex.IsMatch(fileId))
                throw new Exception("Invalid GdriveFileIds");

            var cacheFile = _configuration.FileCacheBasePath + "/" + fileId + "." + sizeSuffix;
            if (!File.Exists(cacheFile))
            {
                var gdriveReq = _driveService.Files.Get(fileId);
                gdriveReq.SupportsAllDrives = true;
                gdriveReq.Fields = "id,thumbnailLink";
                var gdriveRes = await gdriveReq.ExecuteAsync();

                var newThumbnailLink = new Regex(@"=[swh0-9]+$").Replace(gdriveRes.ThumbnailLink, "=" + sizeSuffix);
                await DownloadFile(newThumbnailLink, cacheFile);
            }

            return _configuration.FileCacheBaseUrl + "/" + fileId + "." + sizeSuffix;
        });

        var res = new GetPhotoThumbnailUrlsResponse();
        res.Urls.AddRange(await Task.WhenAll(tasks));

        return res;
    }
}
