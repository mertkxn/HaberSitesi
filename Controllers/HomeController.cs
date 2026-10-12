using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SonGun.Data;
using SonGun.Models;
using SonGun.Models.ViewModels;

namespace SonGun.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private const int PageSize = 6;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? categoryId, int page = 1)
    {
        if (page < 1) page = 1;

        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();

        var query = _context.News
            .Include(n => n.Category)
            .AsNoTracking();

        string? selectedCategoryName = null;
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(n => n.CategoryId == categoryId.Value);
            selectedCategoryName = categories.FirstOrDefault(c => c.Id == categoryId.Value)?.Name;
        }

        query = query.OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (totalPages == 0) totalPages = 1;

        var newsList = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var viewModel = new HomeIndexViewModel
        {
            NewsList = newsList,
            Categories = categories,
            SelectedCategoryId = categoryId,
            SelectedCategoryName = selectedCategoryName,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var news = await _context.News
            .Include(m => m.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (news == null)
        {
            return NotFound();
        }

        return View(news);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
