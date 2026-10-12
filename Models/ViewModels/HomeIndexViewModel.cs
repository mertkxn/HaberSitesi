namespace SonGun.Models.ViewModels;

public class HomeIndexViewModel
{
    public IEnumerable<News> NewsList { get; set; } = Enumerable.Empty<News>();
    public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();
    public int? SelectedCategoryId { get; set; }
    public string? SelectedCategoryName { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
