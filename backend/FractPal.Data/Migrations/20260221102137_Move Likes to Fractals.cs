using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FractPal.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveLikestoFractals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Posts_PostId",
                table: "Likes");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "Likes",
                newName: "FractalId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_PostId",
                table: "Likes",
                newName: "IX_Likes_FractalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Fractals_FractalId",
                table: "Likes",
                column: "FractalId",
                principalTable: "Fractals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Fractals_FractalId",
                table: "Likes");

            migrationBuilder.RenameColumn(
                name: "FractalId",
                table: "Likes",
                newName: "PostId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_FractalId",
                table: "Likes",
                newName: "IX_Likes_PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Posts_PostId",
                table: "Likes",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
