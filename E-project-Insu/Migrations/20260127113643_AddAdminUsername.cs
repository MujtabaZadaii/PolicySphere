using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_project_Insu.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "Admins");
        }
    }
}
