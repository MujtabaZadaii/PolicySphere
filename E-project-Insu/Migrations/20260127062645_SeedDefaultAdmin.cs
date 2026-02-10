using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_project_Insu.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO Admins (Email, Password, Role) VALUES ('Admin@policysphere.com', 'admin', 'Admin')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
