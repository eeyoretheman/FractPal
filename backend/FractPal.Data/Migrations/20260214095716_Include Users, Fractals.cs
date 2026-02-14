using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FractPal.Data.Migrations
{
    /// <inheritdoc />
    public partial class IncludeUsersFractals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Follows",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FollowerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FollowingId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Follows_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Follows_AspNetUsers_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fractals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    Axiom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rules = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Generations = table.Column<int>(type: "int", nullable: false),
                    XTranslation = table.Column<double>(type: "float", nullable: false),
                    YTranslation = table.Column<double>(type: "float", nullable: false),
                    Zoom = table.Column<double>(type: "float", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fractals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fractals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FractalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Fractals_FractalId",
                        column: x => x.FractalId,
                        principalTable: "Fractals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerId_FollowingId",
                table: "Follows",
                columns: new[] { "FollowerId", "FollowingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowingId",
                table: "Follows",
                column: "FollowingId");

            migrationBuilder.CreateIndex(
                name: "IX_Fractals_IsPublished",
                table: "Fractals",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Fractals_PublishedAt",
                table: "Fractals",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Fractals_UserId",
                table: "Fractals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_FractalId",
                table: "Likes",
                column: "FractalId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId_FractalId",
                table: "Likes",
                columns: new[] { "UserId", "FractalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Follows");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "Fractals");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "JoinedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });
        }
    }
}
