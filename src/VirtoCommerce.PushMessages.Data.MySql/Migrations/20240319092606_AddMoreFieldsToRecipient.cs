using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.PushMessages.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreFieldsToRecipient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "PushMessageRecipient",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MemberId",
                table: "PushMessageRecipient",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MemberName",
                table: "PushMessageRecipient",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "PushMessageRecipient",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "PushMessageRecipient");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "PushMessageRecipient");

            migrationBuilder.DropColumn(
                name: "MemberName",
                table: "PushMessageRecipient");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "PushMessageRecipient");
        }
    }
}
