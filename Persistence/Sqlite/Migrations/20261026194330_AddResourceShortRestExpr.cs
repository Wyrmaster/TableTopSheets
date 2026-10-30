using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceShortRestExpr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortRestExpr",
                table: "Resources",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortRestExpr",
                table: "Resources");
        }
    }
}
