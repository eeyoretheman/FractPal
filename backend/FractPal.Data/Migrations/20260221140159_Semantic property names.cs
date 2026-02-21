using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FractPal.Data.Migrations
{
    /// <inheritdoc />
    public partial class Semanticpropertynames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FractalThumbnailPath",
                table: "Fractals",
                newName: "Thumbnail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Thumbnail",
                table: "Fractals",
                newName: "FractalThumbnailPath");
        }
    }
}
