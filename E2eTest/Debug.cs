using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Xunit;
using Xunit.Abstractions;

namespace E2eTest;

public class Debug
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Debug(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Run()
    {
        var credential = GoogleCredential
            .FromFile("/home/martin/p/strelka/api/auth.json")
            .CreateScoped(DriveService.Scope.DriveReadonly);

        var drive = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential
        });
        // d.Drives.List().Execute().Drives.Select(d => d.Name).ToList().ForEach(_testOutputHelper.WriteLine);
        // _testOutputHelper.WriteLine(d.Files.List().Execute().Files.Count.ToString());


        var req = drive.Files.Get("1EdHbPBd5eaw1Oi37GHCLXn12pRe0iBGV");
        req.SupportsAllDrives = true;
        _testOutputHelper.WriteLine(req.Execute().Name);
    }
}
