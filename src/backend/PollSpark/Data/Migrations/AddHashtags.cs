using Microsoft.EntityFrameworkCore.Migrations;

namespace PollSpark.Data.Migrations;

public partial class AddHashtags : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Hashtags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Hashtags", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PollHashtag",
            columns: table => new
            {
                PollsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                HashtagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PollHashtag", x => new { x.PollsId, x.HashtagsId });
                table.ForeignKey(
                    name: "FK_PollHashtag_Hashtags_HashtagsId",
                    column: x => x.HashtagsId,
                    principalTable: "Hashtags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PollHashtag_Polls_PollsId",
                    column: x => x.PollsId,
                    principalTable: "Polls",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Hashtags_Name",
            table: "Hashtags",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PollHashtag_HashtagsId",
            table: "PollHashtag",
            column: "HashtagsId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PollHashtag");

        migrationBuilder.DropTable(
            name: "Hashtags");
    }
} 