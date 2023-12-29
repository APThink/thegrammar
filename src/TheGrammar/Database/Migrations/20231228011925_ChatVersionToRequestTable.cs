using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrammar.Database.Migrations;

/// <inheritdoc />
public partial class ChatVersionToRequestTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ChatVersion",
            table: "Requests",
            type: "TEXT",
            maxLength: 50,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ChatVersion",
            table: "Requests");
    }
}
