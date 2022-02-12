using ApiSpec.Grpc.Media;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class MediaService : Service.ServiceBase
{
    private Data.MainDb _db;
    private readonly ILogger<MediaService> _log;
    private readonly DriveService _driveService;

    public MediaService(
        Data.MainDb db,
        ILogger<MediaService> log,
        DriveService driveService
    )
    {
        _db = db;
        _log = log;
        _driveService = driveService;
    }

    public override Task<CreateGalleryResponse> CreateGallery(CreateGalleryRequest request, ServerCallContext context)
    {
        return base.CreateGallery(request, context);
    }

    public override Task<GetGalleryResponse> GetGallery(GetGalleryRequest request, ServerCallContext context)
    {
        return base.GetGallery(request, context);
    }

    public override Task<GetGalleriesResponse> GetGalleries(GetGalleriesRequest request, ServerCallContext context)
    {
        return base.GetGalleries(request, context);
    }

    public override Task<DeleteGalleryResponse> DeleteGallery(DeleteGalleryRequest request, ServerCallContext context)
    {
        return base.DeleteGallery(request, context);
    }

    public override async Task<GetGalleryPhotosResponse> GetGalleryPhotos(GetGalleryPhotosRequest request, ServerCallContext context)
    {
        var id = Guid.Parse(request.GalleryId.Value);

        var dbGallery = await _db.Galleries.FindAsync(id);
        if (dbGallery == null)
            throw new NotFound($"Gallery {id} not found.");

        var dbGalleryPhotos = await _db.Photos.Where(x => x.GalleryId == id).ToListAsync();

        return new GetGalleryPhotosResponse()
        {
            Photos = { dbGalleryPhotos.Select(p => new Gallery.Types.Photo
            {
                Broken = p.Broken,
                Caption = p.Caption,
                GdriveFileId = p.GdriveFileId,
                Id = p.Id.ToUuid(),
                Order = p.Order
            })}
        };   
        
    }

    public override Task<SyncGalleryPhotosResponse> SyncGalleryPhotos(SyncGalleryPhotosRequest request, ServerCallContext context)
    {
        return base.SyncGalleryPhotos(request, context);
    }
    
    public override async Task<GetFileUrlResponse> GetFileUrl(GetFileUrlRequest request, ServerCallContext context)
    {
        var req = _driveService.Files.Get(request.GdriveFileId);
        req.SupportsAllDrives = true;
        req.Fields = "id,webContentLink";
        var res = await req.ExecuteAsync();

        return new GetFileUrlResponse
        {
            Url = res.WebContentLink
        };
    }

    public override async Task<GetPhotoThumbnailUrlsResponse> GetPhotoThumbnailUrls(GetPhotoThumbnailUrlsRequest request, ServerCallContext context)
    {
        var res = new GetPhotoThumbnailUrlsResponse();

        foreach (var fileId in request.GdriveFileIds)
        {
            var req = _driveService.Files.Get(fileId);
            req.SupportsAllDrives = true;
            req.Fields = "id,thumbnailLink";
            var gdriveRes = await req.ExecuteAsync();
            res.Urls.Add(gdriveRes.ThumbnailLink);
        }

        return res;
    }
}