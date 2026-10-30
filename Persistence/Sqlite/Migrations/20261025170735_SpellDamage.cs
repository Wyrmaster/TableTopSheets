using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class SpellDamage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AttackRoll",
                table: "Spells",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CantripDamage",
                table: "Spells",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Damage",
                table: "Spells",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DamageType",
                table: "Spells",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaveAbility",
                table: "Spells",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttackRoll",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "CantripDamage",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "Damage",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "DamageType",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "SaveAbility",
                table: "Spells");
        }
    }
}
