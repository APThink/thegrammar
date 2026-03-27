using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrammar.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddModelsTableAndRenameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatVersion",
                table: "Requests",
                newName: "ModelKey");

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Temperature = table.Column<float>(type: "REAL", nullable: true),
                    TopP = table.Column<float>(type: "REAL", nullable: false),
                    FrequencyPenalty = table.Column<float>(type: "REAL", nullable: false),
                    PresencePenalty = table.Column<float>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Models_Key",
                table: "Models",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.RenameColumn(
                name: "ModelKey",
                table: "Requests",
                newName: "ChatVersion");
        }
    }
}
