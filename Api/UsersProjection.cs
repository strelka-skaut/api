using System.Diagnostics;
using Api.Data;

namespace Api;

public class UsersProjection : EventBus.ISubscriber<UserCreated>, EventBus.ISubscriber<UserUpdated>, EventBus.ISubscriber<UserDeleted>
{
    private readonly ProjectionDb _db;

    public UsersProjection(ProjectionDb db)
    {
        _db = db;
    }

    public async void Notify(UserCreated ev)
    {
        _db.Users.Add(new User
        {
            Id = ev.Id,
            Firstname = ev.Firstname,
            Lastname = ev.Lastname,
            Email = ev.Email,
        });
        await _db.SaveChangesAsync();
    }

    public async void Notify(UserUpdated ev)
    {
        var user = await _db.Users.FindAsync(ev.Id);
        Debug.Assert(user != null, nameof(user) + " != null");

        if (ev.Firstname != null)
            user.Firstname = ev.Firstname;
        if (ev.Lastname != null)
            user.Lastname = ev.Lastname;
        if (ev.Email != null)
            user.Email = ev.Email;

        await _db.SaveChangesAsync();
    }

    public async void Notify(UserDeleted ev)
    {
        var user = await _db.Users.FindAsync(ev.Id);
        Debug.Assert(user != null, nameof(user) + " != null");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }
}
