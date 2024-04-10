using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBirthdayBot.Migrations
{
    /// <inheritdoc />
    public partial class AddSentMessagesAndSetMaxStringLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Chats",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SentMessages",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentMessages", x => new { x.ChatId, x.MessageId });
                    table.ForeignKey(
                        name: "FK_SentMessages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SentMessages");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Chats",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);
        }
    }
}
