namespace Api;

public static class MyExtensions
{
    public static Uuid ToUuid(this Guid guid)
    {
        return new Uuid
        {
            Value = guid.ToString()
        };
    }
}
