using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatBirthdayBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    ID = table.Column<long>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    ID = table.Column<long>(type: "integer", nullable: false),
                    BirthdayDay = table.Column<byte>(type: "integer", nullable: true),
                    BirthdayMonth = table.Column<byte>(type: "integer", nullable: true),
                    BirthdayYear = table.Column<ushort>(type: "integer", nullable: true),
                    FirstName = table.Column<string>(type: "varchar", nullable: true),
                    LastName = table.Column<string>(type: "varchar", nullable: true),
                    Username = table.Column<string>(type: "varchar", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserChat",
                columns: table => new
                {
                    UserID = table.Column<long>(type: "integer", nullable: false),
                    ChatID = table.Column<long>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChat", x => new { x.UserID, x.ChatID });
                    table.ForeignKey(
                        name: "FK_UserChat_Chat_ChatID",
                        column: x => x.ChatID,
                        principalTable: "Chat",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChat_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UserChat_ChatID",
                table: "UserChat",
                column: "ChatID");

            migrationBuilder.CreateIndex(
                name: "UserChat_UserID",
                table: "UserChat",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChat");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
