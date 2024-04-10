using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBirthdayBot.Migrations
{
    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
    public partial class AddChatPropsAndChangeTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChat_Chat_ChatID",
                table: "UserChat");

            migrationBuilder.DropForeignKey(
                name: "FK_UserChat_User_UserID",
                table: "UserChat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserChat",
                table: "UserChat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat",
                table: "Chat");

            migrationBuilder.RenameTable(
                name: "UserChat",
                newName: "UserChats");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Chat",
                newName: "Chats");

            migrationBuilder.RenameColumn(
                name: "ChatID",
                table: "UserChats",
                newName: "ChatId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "UserChats",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "UserChat_UserID",
                table: "UserChats",
                newName: "IX_UserChats_UserId");

            migrationBuilder.RenameIndex(
                name: "UserChat_ChatID",
                table: "UserChats",
                newName: "IX_UserChats_ChatId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "UserChats",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "ChatId",
                table: "UserChats",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserChats",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldNullable: true);

            migrationBuilder.AlterColumn<ushort>(
                name: "BirthdayYear",
                table: "Users",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ushort),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "BirthdayMonth",
                table: "Users",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "BirthdayDay",
                table: "Users",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Chats",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Locale",
                table: "Chats",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                table: "Chats",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "integer");

            migrationBuilder.AddColumn<byte>(
                name: "CustomOffsetInHours",
                table: "Chats",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldPinNotify",
                table: "Chats",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeZoneOffset",
                table: "Chats",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserChats",
                table: "UserChats",
                columns: new[] { "UserId", "ChatId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chats",
                table: "Chats",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserChats_Chats_ChatId",
                table: "UserChats",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserChats_Users_UserId",
                table: "UserChats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChats_Chats_ChatId",
                table: "UserChats");

            migrationBuilder.DropForeignKey(
                name: "FK_UserChats_Users_UserId",
                table: "UserChats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserChats",
                table: "UserChats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chats",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "CustomOffsetInHours",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ShouldPinNotify",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "TimeZoneOffset",
                table: "Chats");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "UserChats",
                newName: "UserChat");

            migrationBuilder.RenameTable(
                name: "Chats",
                newName: "Chat");

            migrationBuilder.RenameColumn(
                name: "ChatId",
                table: "UserChat",
                newName: "ChatID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserChat",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_UserChats_UserId",
                table: "UserChat",
                newName: "UserChat_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_UserChats_ChatId",
                table: "UserChat",
                newName: "UserChat_ChatID");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "User",
                type: "varchar",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "User",
                type: "varchar",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "User",
                type: "varchar",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<ushort>(
                name: "BirthdayYear",
                table: "User",
                type: "integer",
                nullable: true,
                oldClrType: typeof(ushort),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "BirthdayMonth",
                table: "User",
                type: "integer",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "BirthdayDay",
                table: "User",
                type: "integer",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                table: "User",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "UserChat",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "ChatID",
                table: "UserChat",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "UserID",
                table: "UserChat",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Chat",
                type: "varchar",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Locale",
                table: "Chat",
                type: "varchar",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                table: "Chat",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserChat",
                table: "UserChat",
                columns: new[] { "UserID", "ChatID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat",
                table: "Chat",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserChat_Chat_ChatID",
                table: "UserChat",
                column: "ChatID",
                principalTable: "Chat",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserChat_User_UserID",
                table: "UserChat",
                column: "UserID",
                principalTable: "User",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
