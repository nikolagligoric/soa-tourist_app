using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monitoring.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertInstanceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstanceId",
                table: "Alerts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstanceId",
                table: "Alerts");
        }
    }
}
