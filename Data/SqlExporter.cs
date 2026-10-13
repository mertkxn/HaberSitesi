using System.Text;
using Microsoft.EntityFrameworkCore;
using SonGun.Data;

namespace SonGun.Data;

public static class SqlExporter
{
    public static void ExportToSql(AppDbContext context, string filePath)
    {
        var sb = new StringBuilder();

        sb.AppendLine("-- =============================================");
        sb.AppendLine("-- SON GUN HABER SITESI - VERITABANI KODLARI");
        sb.AppendLine("-- Olusturulma: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
        sb.AppendLine("-- =============================================");
        sb.AppendLine();
        sb.AppendLine("-- 1. KATEGORILER TABLOSU");
        sb.AppendLine("CREATE TABLE IF NOT EXISTS Kategoriler (");
        sb.AppendLine("    Id INTEGER PRIMARY KEY AUTOINCREMENT,");
        sb.AppendLine("    KategoriAdi VARCHAR(100) NOT NULL,");
        sb.AppendLine("    Aciklama TEXT");
        sb.AppendLine(");");
        sb.AppendLine();
        sb.AppendLine("-- 2. HABERLER TABLOSU");
        sb.AppendLine("CREATE TABLE IF NOT EXISTS Haberler (");
        sb.AppendLine("    Id INTEGER PRIMARY KEY AUTOINCREMENT,");
        sb.AppendLine("    KategoriId INTEGER NOT NULL,");
        sb.AppendLine("    Baslik VARCHAR(255) NOT NULL,");
        sb.AppendLine("    Icerik TEXT NOT NULL,");
        sb.AppendLine("    ResimUrl VARCHAR(500),");
        sb.AppendLine("    Yazar VARCHAR(100) NOT NULL,");
        sb.AppendLine("    OlusturulmaTarihi DATETIME NOT NULL,");
        sb.AppendLine("    FOREIGN KEY (KategoriId) REFERENCES Kategoriler(Id) ON DELETE RESTRICT");
        sb.AppendLine(");");
        sb.AppendLine();
        sb.AppendLine("-- =============================================");
        sb.AppendLine("-- VERILER (KATEGORILER VE HABERLER)");
        sb.AppendLine("-- =============================================");
        sb.AppendLine();

        var categories = context.Categories.OrderBy(c => c.Id).ToList();
        foreach (var cat in categories)
        {
            var name = cat.Name.Replace("'", "''");
            var desc = (cat.Description ?? "").Replace("'", "''");
            sb.AppendLine($"INSERT INTO Kategoriler (Id, KategoriAdi, Aciklama) VALUES ({cat.Id}, '{name}', '{desc}');");
        }

        sb.AppendLine();

        var newsList = context.News.OrderBy(n => n.Id).ToList();
        foreach (var news in newsList)
        {
            var title = (news.Title ?? "").Replace("'", "''");
            var content = (news.Content ?? "").Replace("'", "''");
            var img = (news.ImageUrl ?? "").Replace("'", "''");
            var author = (news.Author ?? "").Replace("'", "''");
            var date = news.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

            sb.AppendLine($"INSERT INTO Haberler (Id, KategoriId, Baslik, Icerik, ResimUrl, Yazar, OlusturulmaTarihi) VALUES ({news.Id}, {news.CategoryId}, '{title}', '{content}', '{img}', '{author}', '{date}');");
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }
}
