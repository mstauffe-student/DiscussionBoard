using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarId",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");
        }
    }
}
