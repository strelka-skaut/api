using Api.Data;

namespace Api;

public class EventBus
{
    private EventDb _db;

    private IList<ISubscriber> _subscribers;

    public EventBus(EventDb db)
    {
        _db = db;
        _subscribers = new List<ISubscriber>();
    }

    public void Subscribe(ISubscriber subscriber)
    {
        if (!_subscribers.Contains(subscriber))
            _subscribers.Add(subscriber);
    }

    public void Unsubscribe(ISubscriber subscriber)
    {
        // todo Disposable ISubscriber
        _subscribers.Remove(subscriber);
    }

    public async void Publish(string type, string data)
    {
        var ev = await CreateEvent(type, data);
        foreach (var subscriber in _subscribers)
            subscriber.Notify(ev);
    }

    private async Task<Event> CreateEvent(string type, string data)
    {
        var ev = new Data.Event
        {
            Id = Guid.NewGuid(),
            Type = type,
            Data = data,
            Time = DateTime.UtcNow,
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return ev;
    }

    public abstract class Event
    {
        public Guid Id { get; set; }
        public DateTime Time { get; set; }
    }

    public interface ISubscriber<TEvent> where TEvent : Event
    {
        public void Notify(TEvent ev);
    }
}
