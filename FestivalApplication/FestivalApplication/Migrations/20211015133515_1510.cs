using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace FestivalApplication.Migrations
{
    public partial class _1510 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicList",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ListName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicList", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Stage",
                columns: table => new
                {
                    StageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    StageName = table.Column<string>(type: "text", nullable: true),
                    StageActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Archived = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stage", x => x.StageID);
                });

            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    TrackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TrackName = table.Column<string>(type: "text", nullable: true),
                    TrackSource = table.Column<string>(type: "text", nullable: true),
                    Length = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Track", x => x.TrackID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    PassWord = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "MusicListActivity",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ListID = table.Column<int>(type: "int", nullable: false),
                    StageID = table.Column<int>(type: "int", nullable: false),
                    PreviousSong = table.Column<int>(type: "int", nullable: false),
                    NextSong = table.Column<int>(type: "int", nullable: false),
                    NextNextSong = table.Column<int>(type: "int", nullable: false),
                    Start = table.Column<DateTime>(type: "datetime", nullable: false),
                    Finish = table.Column<DateTime>(type: "datetime", nullable: false),
                    MusicListID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicListActivity", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MusicListActivity_MusicList_MusicListID",
                        column: x => x.MusicListID,
                        principalTable: "MusicList",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackActivity",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MusicListID = table.Column<int>(type: "int", nullable: false),
                    TrackID = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    Playing = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackActivity", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrackActivity_MusicList_MusicListID",
                        column: x => x.MusicListID,
                        principalTable: "MusicList",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Authentication",
                columns: table => new
                {
                    AuthenticationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    AuthenticationKey = table.Column<string>(type: "text", nullable: true),
                    MaxExpiryDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CurrentExpiryDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authentication", x => x.AuthenticationID);
                    table.ForeignKey(
                        name: "FK_Authentication_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyPoints",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyPoints", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LoyaltyPoints_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserActivity",
                columns: table => new
                {
                    UserActivityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    StageID = table.Column<int>(type: "int", nullable: true),
                    Entry = table.Column<DateTime>(type: "datetime", nullable: false),
                    Exit = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivity", x => x.UserActivityID);
                    table.ForeignKey(
                        name: "FK_UserActivity_Stage_StageID",
                        column: x => x.StageID,
                        principalTable: "Stage",
                        principalColumn: "StageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserActivity_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    MessageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MessageText = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    UserActivityID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_Message_UserActivity_UserActivityID",
                        column: x => x.UserActivityID,
                        principalTable: "UserActivity",
                        principalColumn: "UserActivityID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interaction",
                columns: table => new
                {
                    InteractionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    InteractionType = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    UserActivityID = table.Column<int>(type: "int", nullable: true),
                    MessageID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interaction", x => x.InteractionID);
                    table.ForeignKey(
                        name: "FK_Interaction_Message_MessageID",
                        column: x => x.MessageID,
                        principalTable: "Message",
                        principalColumn: "MessageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interaction_UserActivity_UserActivityID",
                        column: x => x.UserActivityID,
                        principalTable: "UserActivity",
                        principalColumn: "UserActivityID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authentication_UserID",
                table: "Authentication",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_MessageID",
                table: "Interaction",
                column: "MessageID");

            migrationBuilder.CreateIndex(
                name: "IX_Interaction_UserActivityID",
                table: "Interaction",
                column: "UserActivityID");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPoints_UserID",
                table: "LoyaltyPoints",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Message_UserActivityID",
                table: "Message",
                column: "UserActivityID");

            migrationBuilder.CreateIndex(
                name: "IX_MusicListActivity_MusicListID",
                table: "MusicListActivity",
                column: "MusicListID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackActivity_MusicListID",
                table: "TrackActivity",
                column: "MusicListID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivity_StageID",
                table: "UserActivity",
                column: "StageID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivity_UserID",
                table: "UserActivity",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Authentication");

            migrationBuilder.DropTable(
                name: "Interaction");

            migrationBuilder.DropTable(
                name: "LoyaltyPoints");

            migrationBuilder.DropTable(
                name: "MusicListActivity");

            migrationBuilder.DropTable(
                name: "Track");

            migrationBuilder.DropTable(
                name: "TrackActivity");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "MusicList");

            migrationBuilder.DropTable(
                name: "UserActivity");

            migrationBuilder.DropTable(
                name: "Stage");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
