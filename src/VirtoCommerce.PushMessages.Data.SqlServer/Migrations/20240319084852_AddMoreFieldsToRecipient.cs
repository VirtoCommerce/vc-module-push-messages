using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.PushMessages.Data.SqlServer.Migrations
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
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MemberId",
                table: "PushMessageRecipient",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberName",
                table: "PushMessageRecipient",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "PushMessageRecipient",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
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
