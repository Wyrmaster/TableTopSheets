using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class StartingEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Group",
                table: "Items",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bundle",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "StartingEquipment",
                table: "Classes",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundEquipment",
                table: "Characters",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassEquipment",
                table: "Characters",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartingEquipment",
                table: "Backgrounds",
                type: "text",
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

            migrationBuilder.AlterColumn<string>(
                name: "Group",
                table: "Items",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
