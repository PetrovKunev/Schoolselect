using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelect.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchoolProfileChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfessionalQualification",
                table: "SchoolProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "SchoolProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "SchoolProfiles",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfessionalQualification",
                table: "SchoolProfiles");

            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "SchoolProfiles");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "SchoolProfiles");
        }
    }
}
