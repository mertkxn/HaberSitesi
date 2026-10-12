using System.ComponentModel.DataAnnotations;

namespace SonGun.Models;

public class News
{
    public int Id { get; set; }

    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required(ErrorMessage = "Başlık gereklidir.")]
    [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
    [Display(Name = "Haber Başlığı")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "İçerik gereklidir.")]
    [Display(Name = "İçerik")]
    public required string Content { get; set; }

    [Display(Name = "Görsel URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Yazar")]
    [StringLength(50)]
    public string Author { get; set; } = "Azrail";

    [Display(Name = "Yayın Tarihi")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
