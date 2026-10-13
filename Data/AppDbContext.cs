using Microsoft.EntityFrameworkCore;
using SonGun.Models;

namespace SonGun.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<News> News => Set<News>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Kategoriler");
            entity.Property(c => c.Id).HasColumnName("Id");
            entity.Property(c => c.Name).HasColumnName("KategoriAdi");
            entity.Property(c => c.Description).HasColumnName("Aciklama");
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.ToTable("Haberler");
            entity.Property(n => n.Id).HasColumnName("Id");
            entity.Property(n => n.CategoryId).HasColumnName("KategoriId");
            entity.Property(n => n.Title).HasColumnName("Baslik");
            entity.Property(n => n.Content).HasColumnName("Icerik");
            entity.Property(n => n.ImageUrl).HasColumnName("ResimUrl");
            entity.Property(n => n.Author).HasColumnName("Yazar");
            entity.Property(n => n.CreatedAt).HasColumnName("OlusturulmaTarihi");

            entity.HasOne(n => n.Category)
                .WithMany(c => c.News)
                .HasForeignKey(n => n.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
