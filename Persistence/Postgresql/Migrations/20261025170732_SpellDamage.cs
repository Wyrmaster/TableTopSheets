using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Postgresql.Migrations
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
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "CantripDamage",
                table: "Spells",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Damage",
                table: "Spells",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DamageType",
                table: "Spells",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaveAbility",
                table: "Spells",
                type: "text",
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
