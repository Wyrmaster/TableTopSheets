using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddMagicItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BaseItemId",
                table: "Items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rarity",
                table: "Items",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_BaseItemId",
                table: "Items",
                column: "BaseItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_BaseItemId",
                table: "Items",
                column: "BaseItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_BaseItemId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_BaseItemId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BaseItemId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Rarity",
                table: "Items");
        }
    }
}
