using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class ProjectionDb : DbContext
{
    public ProjectionDb(DbContextOptions<MainDb> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.LogTo(Console.WriteLine);
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
}

public class User
{
    public Guid Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
}
