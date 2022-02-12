using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace Api;

public class DriveServiceFactory
{
    public DriveService Create()
    {
        var credential = GoogleCredential
            .FromFile("/home/martin/p/strelka/auth.json")
            .CreateScoped(DriveService.Scope.DriveReadonly);

        return new DriveService(new BaseClientService.Initializer() {
            HttpClientInitializer = credential
        });
    }
}
