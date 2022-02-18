using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class EventDb : DbContext
{
    public EventDb(DbContextOptions<MainDb> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.LogTo(Console.WriteLine);
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
}

[Table("Event")]
public class Event
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Data { get; set; } = null!;
    public DateTime Time { get; set; }
}
