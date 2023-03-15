using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBirthdayBot.Migrations
{
    /// <inheritdoc />
    public partial class UseIntegerNumberForTimeZoneOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneOffset",
                table: "Chats");

            migrationBuilder.AddColumn<byte>(
                name: "TimeZoneHourOffset",
                table: "Chats",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneHourOffset",
                table: "Chats");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeZoneOffset",
                table: "Chats",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
