using System.ComponentModel.DataAnnotations;

namespace SonGun.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı gereklidir.")]
    [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Kategori Adı")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<News> News { get; set; } = new List<News>();
}
