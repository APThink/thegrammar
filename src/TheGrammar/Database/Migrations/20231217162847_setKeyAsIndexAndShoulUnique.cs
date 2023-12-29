using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrammar.Database.Migrations;

/// <inheritdoc />
public partial class setKeyAsIndexAndShoulUnique : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Prompts_LeftKey_RightKey",
            table: "Prompts",
            columns: new[] { "LeftKey", "RightKey" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Prompts_LeftKey_RightKey",
            table: "Prompts");
    }
}
