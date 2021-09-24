using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace FestivalApplication.Migrations
{
    public partial class StageAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stage",
                columns: table => new
                {
                    StageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    StageName = table.Column<string>(type: "text", nullable: true),
                    StageActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stage", x => x.StageID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivity_StageID",
                table: "UserActivity",
                column: "StageID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivity_Stage_StageID",
                table: "UserActivity",
                column: "StageID",
                principalTable: "Stage",
                principalColumn: "StageID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivity_Stage_StageID",
                table: "UserActivity");

            migrationBuilder.DropTable(
                name: "Stage");

            migrationBuilder.DropIndex(
                name: "IX_UserActivity_StageID",
                table: "UserActivity");
        }
    }
}
