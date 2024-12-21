using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODExplorer.Migrations
{
    /// <inheritdoc />
    public partial class EdAstroMarkDown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MarkDown",
                table: "EdAstroPois",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkDown",
                table: "EdAstroPois");
        }
    }
}
