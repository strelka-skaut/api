using ApiSpec.Grpc.Events;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class EventService : Service.ServiceBase
{
    private readonly Data.MainDb          _db;
    private readonly ILogger<EventService> _log;

    public EventService(
        Data.MainDb          db,
        ILogger<EventService> log
    )
    {
        _db  = db;
        _log = log;
    }

    public override async Task<GetEventResponse> GetEvent(GetEventRequest request, ServerCallContext context)
    {
        var id = request.EventId.ToGuid();

        var dbEvent = await _db.Events.FindAsync(id);
        if (dbEvent == null)
            throw new NotFound($"Event {id} not found.");

        return new GetEventResponse
        {
            Event = new Event
            {
                Id   = dbEvent.Id.ToUuid(),
                Name = dbEvent.Name,
                Slug = dbEvent.Slug,
            },
        };
    }

    public override async Task<GetEventsResponse> GetEvents(
        GetEventsRequest   request,
        ServerCallContext context
    )
    {
        if (request.Limit > 1000)
            throw new InvalidArgument("Limit cannot be greater than 1000.");

        var dbEvents = await _db.Events
            .OrderBy(x => x.Id)
            .Skip(request.HasOffset ? request.Offset : 0)
            .Take(request.HasLimit ? request.Limit : 25)
            .ToListAsync();

        var resp = new GetEventsResponse();
        foreach (var site in dbEvents)
        {
            resp.Events.Add(new Event
            {
                Id   = site.Id.ToUuid(),
                Name = site.Name,
                Slug = site.Slug,
            });
        }

        return resp;
    }

    public override async Task<CreateEventResponse> CreateEvent(
        CreateEventRequest request,
        ServerCallContext context
    )
    {
        var id = Guid.NewGuid();

        _db.Events.Add(new Data.Event
        {
            Id   = id,
            Name = request.Name,
            Slug = request.Slug,
        });

        await _db.SaveChangesAsync();

        return new CreateEventResponse
        {
            Id = id.ToUuid(),
        };
    }

    public override async Task<UpdateEventResponse> UpdateEvent(UpdateEventRequest request, ServerCallContext context)
    {
        var id = request.EventId.ToGuid();

        var dbEvent = await _db.Events.FindAsync(id);
        if (dbEvent == null)
            throw new NotFound($"Event {id} not found.");

        if (request.HasName)
            dbEvent.Name = request.Name;

        if (request.HasSlug)
            dbEvent.Slug = request.Slug;

        await _db.SaveChangesAsync();

        return new UpdateEventResponse();
    }

    public override async Task<DeleteEventResponse> DeleteEvent(
        DeleteEventRequest request,
        ServerCallContext context
    )
    {
        var id = request.EventId.ToGuid();
        var site = await _db.Events.FindAsync(id);
        if (site == null)
            throw new NotFound($"Event {id} not found.");

        _db.Remove(site);
        await _db.SaveChangesAsync();

        return new DeleteEventResponse();
    }
}
