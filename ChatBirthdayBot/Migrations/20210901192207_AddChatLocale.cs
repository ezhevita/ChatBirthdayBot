using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatBirthdayBot.Migrations
{
    public partial class AddChatLocale : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Chat",
                type: "varchar",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Chat");
        }
    }
}
