using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace Api;

public class GoogleApiServiceFactory
{
    private string _serviceAccountCredentialFile;

    public GoogleApiServiceFactory(string serviceAccountCredentialFile)
    {
        _serviceAccountCredentialFile = serviceAccountCredentialFile;
    }

    public DriveService CreateDriveService()
    {
        var credential = GoogleCredential
            .FromFile(_serviceAccountCredentialFile)
            .CreateScoped(DriveService.Scope.DriveReadonly);

        return new DriveService(new BaseClientService.Initializer() {
            HttpClientInitializer = credential
        });
    }
}
