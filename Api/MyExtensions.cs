using ApiSpec.Grpc;

namespace Api;

public static class MyExtensions
{
    public static Uuid ToUuid(this Guid guid)
    {
        return new Uuid {Value = guid.ToString()};
    }

    public static Guid ToGuid(this Uuid uuid)
    {
        return Guid.Parse(uuid.Value);
    }
}
