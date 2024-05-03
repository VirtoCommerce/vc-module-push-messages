using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.PushMessages.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreFieldsToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberQuery",
                table: "PushMessage",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrackNewRecipients",
                table: "PushMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberQuery",
                table: "PushMessage");

            migrationBuilder.DropColumn(
                name: "TrackNewRecipients",
                table: "PushMessage");
        }
    }
}
