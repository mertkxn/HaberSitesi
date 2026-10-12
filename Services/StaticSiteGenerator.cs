using System.Text;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;
using SonGun.Data;
using SonGun.Models;

namespace SonGun.Services;

public static class StaticSiteGenerator
{
    public static void Generate(AppDbContext context, string outputDir = "docs")
    {
        var rootPath = Directory.GetCurrentDirectory();
        var outputPath = Path.Combine(rootPath, outputDir);

        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }
        Directory.CreateDirectory(outputPath);

        // Copy wwwroot assets to output directory
        var wwwrootPath = Path.Combine(rootPath, "wwwroot");
        if (Directory.Exists(wwwrootPath))
        {
            CopyDirectory(wwwrootPath, outputPath);
        }

        // Add .nojekyll file for GitHub Pages compatibility
        File.WriteAllText(Path.Combine(outputPath, ".nojekyll"), "");

        // Fetch data
        var categories = context.Categories.AsNoTracking().OrderBy(c => c.Id).ToList();
        var newsList = context.News
            .Include(n => n.Category)
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        // 1. Generate index.html
        var indexHtml = BuildIndexHtml(categories, newsList);
        File.WriteAllText(Path.Combine(outputPath, "index.html"), indexHtml, Encoding.UTF8);

        // 2. Generate details-{id}.html for each news item
        foreach (var item in newsList)
        {
            var detailHtml = BuildDetailsHtml(item);
            File.WriteAllText(Path.Combine(outputPath, $"details-{item.Id}.html"), detailHtml, Encoding.UTF8);
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists) return;

        Directory.CreateDirectory(destinationDir);

        foreach (var file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        foreach (var subDir in dir.GetDirectories())
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    private static string BuildHead(string title)
    {
        var encodedTitle = HtmlEncoder.Default.Encode(title);
        return $$"""
<!DOCTYPE html>
<html lang="tr" data-bs-theme="auto">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>{{encodedTitle}} - SON GÜN</title>
    
    <!-- Theme Auto-Detector for Bootstrap 5 -->
    <script>
        (function () {
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            document.documentElement.setAttribute('data-bs-theme', prefersDark ? 'dark' : 'light');
        })();
    </script>
    
    <!-- CSS -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="css/site.css" />
    
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
    
    <!-- AOS Animation CSS -->
    <link href="https://unpkg.com/aos@2.3.1/dist/aos.css" rel="stylesheet" />
</head>
<body class="d-flex flex-column min-vh-100">
    <header data-aos="fade-down">
        <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-custom border-bottom box-shadow mb-4 shadow">
            <div class="container-fluid">
                <a class="navbar-brand d-flex align-items-center" href="index.html">
                    <img src="images/logo.png" alt="Logo" class="brand-logo" onerror="this.style.display='none'" />
                    SON GÜN
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse" aria-controls="navbarSupportedContent"
                        aria-expanded="false" aria-label="Toggle navigation" style="border-color: white;">
                    <span class="navbar-toggler-icon" style="filter: invert(1);"></span>
                </button>
                <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
                    <ul class="navbar-nav flex-grow-1">
                        <li class="nav-item">
                            <a class="nav-link" href="index.html"><i class="fas fa-home me-1"></i> Ana Sayfa</a>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>
    </header>
    
    <div class="container flex-grow-1">
        <main role="main" class="pb-5">
""";
    }

    private static string BuildFooter()
    {
        return """
        </main>
    </div>

    <footer class="footer-custom mt-auto">
        <div class="container text-center position-relative py-3">
            &copy; 2026 - SON GÜN - Tüm Hakları Saklıdır
        </div>
    </footer>
    
    <!-- Scripts -->
    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="js/site.js"></script>
    
    <!-- AOS Animation JS -->
    <script src="https://unpkg.com/aos@2.3.1/dist/aos.js"></script>
    <script>
        AOS.init({
            duration: 800,
            easing: 'ease-in-out',
            once: true,
            mirror: false
        });
        
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
            document.documentElement.setAttribute('data-bs-theme', event.matches ? 'dark' : 'light');
        });
    </script>
</body>
</html>
""";
    }

    private static string BuildIndexHtml(List<Category> categories, List<News> newsList)
    {
        var sb = new StringBuilder();
        sb.Append(BuildHead("Ana Sayfa"));

        sb.Append("""

<div class="text-center mb-4" data-aos="zoom-in">
    <h1 class="display-4 fw-bold theme-heading">SON GÜN</h1>
    <p class="lead text-muted-custom">Tarihin en kritik dönüm noktaları, gizemli ölümler ve son anlar...</p>
    <hr class="w-50 mx-auto theme-divider" />
</div>

<!-- Kategori Filtreleme Butonları -->
<div class="d-flex flex-wrap justify-content-center gap-2 mb-4" data-aos="fade-up">
    <button type="button" class="btn btn-sm rounded-pill px-3 py-2 btn-light text-dark fw-bold filter-btn active" data-category="all">
        <i class="fas fa-layer-group me-1"></i> Tümü
    </button>
""");

        foreach (var cat in categories)
        {
            var catNameEscaped = HtmlEncoder.Default.Encode(cat.Name);
            var catDescEscaped = HtmlEncoder.Default.Encode(cat.Description ?? "");
            sb.Append($$"""

    <button type="button" class="btn btn-sm rounded-pill px-3 py-2 btn-outline-secondary filter-btn" 
            data-category="{{cat.Id}}" data-category-name="{{catNameEscaped}}" title="{{catDescEscaped}}">
        <i class="fas fa-tag me-1"></i> {{catNameEscaped}}
    </button>
""");
        }

        sb.Append("""

</div>

<div id="filterAlert" class="alert alert-dark border-secondary d-none justify-content-between align-items-center mb-4">
    <span><i class="fas fa-filter text-danger me-2"></i><strong id="filterCategoryText"></strong> kategorisindeki haberler gösteriliyor.</span>
    <button type="button" id="clearFilterBtn" class="btn btn-sm btn-outline-light">Filtreyi Temizle</button>
</div>

<div class="row" id="newsContainer">
""");

        int delay = 0;
        foreach (var news in newsList)
        {
            var title = HtmlEncoder.Default.Encode(news.Title);
            var author = HtmlEncoder.Default.Encode(news.Author);
            var categoryName = HtmlEncoder.Default.Encode(news.Category?.Name ?? "Genel");
            var excerpt = news.Content.Length > 120 ? news.Content.Substring(0, 120) + "..." : news.Content;
            var excerptEscaped = HtmlEncoder.Default.Encode(excerpt);
            
            // Normalize image path for relative usage
            string imageUrl = news.ImageUrl ?? "";
            if (imageUrl.StartsWith("/"))
            {
                imageUrl = imageUrl.Substring(1);
            }

            sb.Append($$"""

    <div class="col-md-6 col-lg-4 mb-4 news-card-wrapper" data-category-id="{{news.CategoryId}}" data-aos="fade-up" data-aos-delay="{{delay}}">
        <div class="card card-theme h-100 shadow-sm rounded-3 overflow-hidden d-flex flex-column">
""");

            if (!string.IsNullOrEmpty(imageUrl))
            {
                sb.Append($$"""

            <div class="position-relative overflow-hidden" style="height: 310px; background-color: #161616;">
                <img src="{{imageUrl}}" 
                     class="card-img-top w-100 h-100" 
                     alt="{{title}}" 
                     style="object-fit: cover; object-position: top center; transition: transform 0.3s ease;" 
                     onmouseover="this.style.transform='scale(1.04)'" 
                     onmouseout="this.style.transform='scale(1)'" 
                     onerror="this.src='images/logo.png'">
            </div>
""");
            }
            else
            {
                sb.Append("""

            <div class="card-img-top bg-secondary d-flex justify-content-center align-items-center" style="height: 310px;">
                <i class="fas fa-skull-crossbones fa-4x text-light opacity-75"></i>
            </div>
""");
            }

            sb.Append($$"""

            <div class="card-body d-flex flex-column">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="badge bg-danger rounded-pill px-2 py-1">
                        <i class="fas fa-tag me-1"></i>{{categoryName}}
                    </span>
                    <span class="badge bg-dark border border-secondary rounded-pill px-2 py-1 text-light">
                        <i class="fas fa-skull me-1 text-danger"></i>{{author}}
                    </span>
                </div>
                <h5 class="card-title fw-bold theme-heading mt-2">{{title}}</h5>
                <p class="card-text text-muted-custom mt-2 flex-grow-1">
                    {{excerptEscaped}}
                </p>
            </div>
            <div class="card-footer card-footer-theme border-0 text-center pb-3">
                <a href="details-{{news.Id}}.html" class="btn btn-theme-outline w-100 py-2">
                    Devamını Oku <i class="fas fa-arrow-right ms-1"></i>
                </a>
            </div>
        </div>
    </div>
""");

            delay += 50;
            if (delay > 400) delay = 100;
        }

        sb.Append("""

</div>

<script>
    document.addEventListener('DOMContentLoaded', function () {
        const filterBtns = document.querySelectorAll('.filter-btn');
        const cards = document.querySelectorAll('.news-card-wrapper');
        const filterAlert = document.getElementById('filterAlert');
        const filterCategoryText = document.getElementById('filterCategoryText');
        const clearFilterBtn = document.getElementById('clearFilterBtn');

        function setCategory(categoryId, categoryName) {
            filterBtns.forEach(btn => {
                if (btn.getAttribute('data-category') === categoryId) {
                    btn.classList.remove('btn-outline-secondary');
                    btn.classList.add(categoryId === 'all' ? 'btn-light' : 'btn-danger', 'fw-bold', 'active');
                    if (categoryId === 'all') btn.classList.add('text-dark');
                } else {
                    btn.classList.remove('btn-light', 'btn-danger', 'text-dark', 'fw-bold', 'active');
                    btn.classList.add('btn-outline-secondary');
                }
            });

            if (categoryId === 'all') {
                filterAlert.classList.remove('d-flex');
                filterAlert.classList.add('d-none');
                cards.forEach(card => card.classList.remove('d-none'));
            } else {
                filterCategoryText.textContent = categoryName || '';
                filterAlert.classList.remove('d-none');
                filterAlert.classList.add('d-flex');
                cards.forEach(card => {
                    if (card.getAttribute('data-category-id') === categoryId) {
                        card.classList.remove('d-none');
                    } else {
                        card.classList.add('d-none');
                    }
                });
            }

            if (window.AOS) {
                AOS.refresh();
            }
        }

        filterBtns.forEach(btn => {
            btn.addEventListener('click', function () {
                const cat = this.getAttribute('data-category');
                const catName = this.getAttribute('data-category-name');
                setCategory(cat, catName);
            });
        });

        if (clearFilterBtn) {
            clearFilterBtn.addEventListener('click', function () {
                setCategory('all');
            });
        }
    });
</script>
""");

        sb.Append(BuildFooter());
        return sb.ToString();
    }

    private static string BuildDetailsHtml(News news)
    {
        var sb = new StringBuilder();
        var title = HtmlEncoder.Default.Encode(news.Title);
        var author = HtmlEncoder.Default.Encode(news.Author);
        var categoryName = HtmlEncoder.Default.Encode(news.Category?.Name ?? "Ölüm Sebebi Belirtilmemiş");
        var dateStr = news.CreatedAt.ToLocalTime().ToString("dd MMMM yyyy HH:mm");
        var content = HtmlEncoder.Default.Encode(news.Content);

        string imageUrl = news.ImageUrl ?? "";
        if (imageUrl.StartsWith("/"))
        {
            imageUrl = imageUrl.Substring(1);
        }

        sb.Append(BuildHead(news.Title));

        sb.Append($$"""

<div class="row justify-content-center">
    <div class="col-lg-9 col-md-11" data-aos="fade-up">
        <a href="index.html" class="btn btn-theme-outline mb-4">
            <i class="fas fa-arrow-left me-1"></i> Tüm Haberlere Dön
        </a>

        <article class="card card-theme shadow-sm rounded-3 overflow-hidden p-4 p-md-5">
            <div class="mb-3">
                <span class="badge bg-danger rounded-pill px-3 py-2 fs-6">
                    <i class="fas fa-tag me-1"></i>{{categoryName}}
                </span>
            </div>

            <h1 class="display-6 fw-bold theme-heading mb-3">{{title}}</h1>

            <div class="d-flex flex-wrap align-items-center gap-3 text-muted-custom mb-4 pb-3 border-bottom" style="border-color: var(--border-color) !important;">
                <span>
                    <i class="fas fa-skull me-1 text-danger"></i> Yazar: <strong>{{author}}</strong>
                </span>
                <span>•</span>
                <span>
                    <i class="far fa-clock me-1"></i> {{dateStr}}
                </span>
            </div>
""");

        if (!string.IsNullOrEmpty(imageUrl))
        {
            sb.Append($$"""

            <div class="mb-4 text-center rounded-3 overflow-hidden shadow-sm" style="background-color: #121212;">
                <img src="{{imageUrl}}" 
                     class="img-fluid rounded-3" 
                     style="max-height: 540px; width: auto; object-fit: contain; margin: 0 auto; display: block;" 
                     alt="{{title}}" 
                     onerror="this.src='images/logo.png'" />
            </div>
""");
        }

        sb.Append($$"""

            <div class="lead" style="white-space: pre-line; line-height: 1.8;">
{{content}}
            </div>
        </article>
    </div>
</div>
""");

        sb.Append(BuildFooter());
        return sb.ToString();
    }
}
