using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class MultiKeyed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "transfers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "stop_times",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "calendar_dates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transfers",
                table: "transfers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stop_times",
                table: "stop_times",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_calendar_dates",
                table: "calendar_dates",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_transfers",
                table: "transfers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stop_times",
                table: "stop_times");

            migrationBuilder.DropPrimaryKey(
                name: "PK_calendar_dates",
                table: "calendar_dates");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "transfers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "stop_times");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "calendar_dates");
        }
    }
}
