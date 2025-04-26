using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelect.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdmissionFormula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BelExamMultiplier",
                table: "AdmissionFormulas");

            migrationBuilder.DropColumn(
                name: "BelGradeMultiplier",
                table: "AdmissionFormulas");

            migrationBuilder.DropColumn(
                name: "KmitGradeMultiplier",
                table: "AdmissionFormulas");

            migrationBuilder.DropColumn(
                name: "MatExamMultiplier",
                table: "AdmissionFormulas");

            migrationBuilder.DropColumn(
                name: "MatGradeMultiplier",
                table: "AdmissionFormulas");

            migrationBuilder.RenameColumn(
                name: "IsStructured",
                table: "AdmissionFormulas",
                newName: "HasComponents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasComponents",
                table: "AdmissionFormulas",
                newName: "IsStructured");

            migrationBuilder.AddColumn<double>(
                name: "BelExamMultiplier",
                table: "AdmissionFormulas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BelGradeMultiplier",
                table: "AdmissionFormulas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "KmitGradeMultiplier",
                table: "AdmissionFormulas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MatExamMultiplier",
                table: "AdmissionFormulas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MatGradeMultiplier",
                table: "AdmissionFormulas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
