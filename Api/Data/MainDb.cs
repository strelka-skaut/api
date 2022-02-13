using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class MainDb : DbContext
{
    public MainDb(DbContextOptions<MainDb> options):base(options){}
    public DbSet<Site> Sites { get; set; }
    public DbSet<Layout> Layouts { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<Photo> Photos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
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

[Table("Gallery")]
public class Gallery
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string GdriveFolderId { get; set; }
    public Guid? SiteId { get; set; }
    public Site? Site { get; set; }
}

[Table("Photo")]
public class Photo
{
    public Guid Id { get; set; }
    public string GdriveFileId { get; set; }
    public int Order { get; set; }
    public string Caption { get; set; }
    public bool Broken { get; set; }
    public Guid GalleryId { get; set; }
    public Gallery Gallery { get; set; }
}
