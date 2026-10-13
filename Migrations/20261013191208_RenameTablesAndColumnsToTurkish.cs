using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SonGun.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesAndColumnsToTurkish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_Categories_CategoryId",
                table: "News");

            migrationBuilder.DropPrimaryKey(
                name: "PK_News",
                table: "News");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "News",
                newName: "Haberler");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Kategoriler");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Haberler",
                newName: "Baslik");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Haberler",
                newName: "ResimUrl");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Haberler",
                newName: "OlusturulmaTarihi");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Haberler",
                newName: "Icerik");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Haberler",
                newName: "KategoriId");

            migrationBuilder.RenameColumn(
                name: "Author",
                table: "Haberler",
                newName: "Yazar");

            migrationBuilder.RenameIndex(
                name: "IX_News_CategoryId",
                table: "Haberler",
                newName: "IX_Haberler_KategoriId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Kategoriler",
                newName: "KategoriAdi");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Kategoriler",
                newName: "Aciklama");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Haberler",
                table: "Haberler",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kategoriler",
                table: "Kategoriler",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Haberler_Kategoriler_KategoriId",
                table: "Haberler",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Haberler_Kategoriler_KategoriId",
                table: "Haberler");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kategoriler",
                table: "Kategoriler");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Haberler",
                table: "Haberler");

            migrationBuilder.RenameTable(
                name: "Kategoriler",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "Haberler",
                newName: "News");

            migrationBuilder.RenameColumn(
                name: "KategoriAdi",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Aciklama",
                table: "Categories",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Yazar",
                table: "News",
                newName: "Author");

            migrationBuilder.RenameColumn(
                name: "ResimUrl",
                table: "News",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "OlusturulmaTarihi",
                table: "News",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "KategoriId",
                table: "News",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "Icerik",
                table: "News",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "Baslik",
                table: "News",
                newName: "Title");

            migrationBuilder.RenameIndex(
                name: "IX_Haberler_KategoriId",
                table: "News",
                newName: "IX_News_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_News",
                table: "News",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_News_Categories_CategoryId",
                table: "News",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
