using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "SchoolFacilities",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DistanceToStop",
                table: "SchoolFacilities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineNumber",
                table: "SchoolFacilities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportMode",
                table: "SchoolFacilities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "SchoolFacilities");

            migrationBuilder.DropColumn(
                name: "DistanceToStop",
                table: "SchoolFacilities");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "SchoolFacilities");

            migrationBuilder.DropColumn(
                name: "TransportMode",
                table: "SchoolFacilities");
        }
    }
}
