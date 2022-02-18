namespace Api;

public class UserCreated : EventBus.Event
{
    public Guid Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
}

public class UserUpdated : EventBus.Event
{
    public Guid Id { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Email { get; set; }
}

public class UserDeleted : EventBus.Event
{
}
