using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class StartingEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bundle",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "StartingEquipment",
                table: "Classes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundEquipment",
                table: "Characters",
                type: "TEXT",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassEquipment",
                table: "Characters",
                type: "TEXT",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartingEquipment",
                table: "Backgrounds",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bundle",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "StartingEquipment",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "BackgroundEquipment",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ClassEquipment",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "StartingEquipment",
                table: "Backgrounds");
        }
    }
}
