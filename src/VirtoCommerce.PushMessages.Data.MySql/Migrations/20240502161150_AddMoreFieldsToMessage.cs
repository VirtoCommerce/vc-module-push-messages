using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.PushMessages.Data.MySql.Migrations
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
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "TrackNewRecipients",
                table: "PushMessage",
                type: "tinyint(1)",
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
