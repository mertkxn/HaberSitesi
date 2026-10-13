using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SonGun.Data;
using SonGun.Models;
using SonGun.Models.ViewModels;

namespace SonGun.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var newsList = await _context.News
            .Include(n => n.Category)
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return View(newsList);
    }

    public async Task<IActionResult> Create()
    {
        var model = new NewsCreateEditViewModel
        {
            Categories = await GetCategorySelectListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewsCreateEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var news = new News
            {
                CategoryId = model.CategoryId,
                Title = model.Title,
                Content = model.Content,
                ImageUrl = model.ImageUrl,
                Author = "Azrail",
                CreatedAt = DateTime.UtcNow
            };

            _context.News.Add(news);
            await _context.SaveChangesAsync();
            SqlExporter.ExportToSql(_context, "database.sql");
            return RedirectToAction(nameof(Index));
        }

        model.Categories = await GetCategorySelectListAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var news = await _context.News.FindAsync(id);
        if (news == null) return NotFound();

        var model = new NewsCreateEditViewModel
        {
            Id = news.Id,
            CategoryId = news.CategoryId,
            Title = news.Title,
            Content = news.Content,
            ImageUrl = news.ImageUrl,
            Categories = await GetCategorySelectListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NewsCreateEditViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var existingNews = await _context.News.FindAsync(id);
            if (existingNews == null)
            {
                return NotFound();
            }

            existingNews.CategoryId = model.CategoryId;
            existingNews.Title = model.Title;
            existingNews.Content = model.Content;
            existingNews.ImageUrl = model.ImageUrl;
            existingNews.Author = "Azrail";

            await _context.SaveChangesAsync();
            SqlExporter.ExportToSql(_context, "database.sql");
            return RedirectToAction(nameof(Index));
        }

        model.Categories = await GetCategorySelectListAsync();
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var news = await _context.News
            .Include(n => n.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (news == null) return NotFound();

        return View(news);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            SqlExporter.ExportToSql(_context, "database.sql");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();
    }
}
