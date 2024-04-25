using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.PushMessages.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddStartDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "PushMessage",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PushMessage",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "PushMessage",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql("UPDATE PushMessage SET Status = 'Sent';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PushMessage");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PushMessage");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "PushMessage");
        }
    }
}
