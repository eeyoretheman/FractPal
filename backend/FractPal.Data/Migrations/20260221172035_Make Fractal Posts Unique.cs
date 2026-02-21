using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FractPal.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeFractalPostsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_FractalId",
                table: "Posts");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_FractalId",
                table: "Posts",
                column: "FractalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_FractalId",
                table: "Posts");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_FractalId",
                table: "Posts",
                column: "FractalId");
        }
    }
}
