using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODExplorer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartoIgnoredSystems",
                columns: table => new
                {
                    Address = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartoIgnoredSystems", x => x.Address);
                });

            migrationBuilder.CreateTable(
                name: "JournalCommanders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    JournalDir = table.Column<string>(type: "TEXT", nullable: false),
                    LastFile = table.Column<string>(type: "TEXT", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalCommanders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Filename = table.Column<string>(type: "TEXT", nullable: false),
                    Offset = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CommanderID = table.Column<int>(type: "INTEGER", nullable: false),
                    EventTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventData = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => new { x.Filename, x.Offset });
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: true),
                    DoubleValue = table.Column<double>(type: "REAL", nullable: true),
                    StringValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommanderIgnoredSystems",
                columns: table => new
                {
                    CartoIgnoredSystemsDTOAddress = table.Column<long>(type: "INTEGER", nullable: false),
                    CommandersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommanderIgnoredSystems", x => new { x.CartoIgnoredSystemsDTOAddress, x.CommandersId });
                    table.ForeignKey(
                        name: "FK_CommanderIgnoredSystems_CartoIgnoredSystems_CartoIgnoredSystemsDTOAddress",
                        column: x => x.CartoIgnoredSystemsDTOAddress,
                        principalTable: "CartoIgnoredSystems",
                        principalColumn: "Address",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommanderIgnoredSystems_JournalCommanders_CommandersId",
                        column: x => x.CommandersId,
                        principalTable: "JournalCommanders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartoIgnoredSystems_Address",
                table: "CartoIgnoredSystems",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_CommanderIgnoredSystems_CommandersId",
                table: "CommanderIgnoredSystems",
                column: "CommandersId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalCommanders_Name",
                table: "JournalCommanders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EventTypeId",
                table: "JournalEntries",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_TimeStamp_Offset",
                table: "JournalEntries",
                columns: new[] { "TimeStamp", "Offset" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommanderIgnoredSystems");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "CartoIgnoredSystems");

            migrationBuilder.DropTable(
                name: "JournalCommanders");
        }
    }
}
