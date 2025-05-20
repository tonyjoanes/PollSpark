using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PollSpark.Migrations
{
    /// <inheritdoc />
    public partial class AddPollCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Votes_PollId",
                table: "Votes");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollCategories",
                columns: table => new
                {
                    CategoriesId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PollsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollCategories", x => new { x.CategoriesId, x.PollsId });
                    table.ForeignKey(
                        name: "FK_PollCategories_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PollCategories_Polls_PollsId",
                        column: x => x.PollsId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PollId_IpAddress",
                table: "Votes",
                columns: new[] { "PollId", "IpAddress" },
                unique: true,
                filter: "[IpAddress] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PollId_UserId",
                table: "Votes",
                columns: new[] { "PollId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PollCategories_PollsId",
                table: "PollCategories",
                column: "PollsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Votes_PollId_IpAddress",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_PollId_UserId",
                table: "Votes");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PollId",
                table: "Votes",
                column: "PollId");
        }
    }
}
