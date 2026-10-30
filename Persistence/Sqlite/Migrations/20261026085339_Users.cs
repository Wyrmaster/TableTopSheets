using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // characters created before users existed have no owner and are removed (child tables first)
            foreach (string table in new[]
                     {
                         "CharacterChoices", "CharacterItems", "CharacterPreparedSpells", "CharacterResourceUsages",
                         "CharacterSpellbookEntries", "CharacterLevels", "Characters",
                     })
            {
                migrationBuilder.Sql($"DELETE FROM \"{table}\";");
            }

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Characters",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ExternalIssuer = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ExternalSubject = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_OwnerId",
                table: "Characters",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalIssuer_ExternalSubject",
                table: "Users",
                columns: new[] { "ExternalIssuer", "ExternalSubject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Users_OwnerId",
                table: "Characters",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Users_OwnerId",
                table: "Characters");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Characters_OwnerId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Characters");
        }
    }
}
