using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddRulesetMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RulesetMigrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ToolVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RulesetMigrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RulesetMigrations_AppliedAt",
                table: "RulesetMigrations",
                column: "AppliedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RulesetMigrations");
        }
    }
}
