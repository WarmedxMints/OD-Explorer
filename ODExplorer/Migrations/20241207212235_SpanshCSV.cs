using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODExplorer.Migrations
{
    /// <inheritdoc />
    public partial class SpanshCSV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpanshCsvs",
                columns: table => new
                {
                    CsvType = table.Column<int>(type: "INTEGER", nullable: false),
                    CommanderID = table.Column<int>(type: "INTEGER", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpanshCsvs", x => new { x.CsvType, x.CommanderID });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpanshCsvs");
        }
    }
}
