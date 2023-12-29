using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrammar.Database.Migrations;

/// <inheritdoc />
public partial class ChangeRequestsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Request",
            table: "Requests");

        migrationBuilder.DropColumn(
            name: "Response",
            table: "Requests");

        migrationBuilder.AddColumn<string>(
            name: "RequestText",
            table: "Requests",
            type: "TEXT",
            maxLength: 5000,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "ResponseText",
            table: "Requests",
            type: "TEXT",
            maxLength: 5000,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RequestText",
            table: "Requests");

        migrationBuilder.DropColumn(
            name: "ResponseText",
            table: "Requests");

        migrationBuilder.AddColumn<string>(
            name: "Request",
            table: "Requests",
            type: "TEXT",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Response",
            table: "Requests",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }
}
