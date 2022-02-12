using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class MainDb : DbContext
{
    public DbSet<Site> Sites { get; set; }
    public DbSet<Layout> Layouts { get; set; }
    public DbSet<Page> Pages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // options.UseNpgsql("server=localhost;port=5432;user id=root;password=root;database=main");
        options.UseNpgsql("server=localhost;port=5433;user id=root;password=root;database=main");
        options.LogTo(Console.WriteLine);
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
}

[Table("Site")]
public class Site
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public Guid LayoutId { get; set; }
    public Layout Layout { get; set; }
}

[Table("Layout")]
public class Layout
{
    public Guid Id { get; set; }
    public string Content { get; set; }
}

[Table("Page")]
public class Page
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedUserId { get; set; }
    public Guid? SiteId { get; set; }
    public Site? Site { get; set; }
}
