using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class Keyless : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_stop_times",
                table: "stop_times");

            migrationBuilder.DropPrimaryKey(
                name: "PK_frequencies",
                table: "frequencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_calendar_dates",
                table: "calendar_dates");

            migrationBuilder.AlterColumn<string>(
                name: "TripId",
                table: "stop_times",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TripId",
                table: "frequencies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceId",
                table: "calendar_dates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TripId",
                table: "stop_times",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TripId",
                table: "frequencies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceId",
                table: "calendar_dates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_stop_times",
                table: "stop_times",
                column: "TripId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_frequencies",
                table: "frequencies",
                column: "TripId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_calendar_dates",
                table: "calendar_dates",
                column: "ServiceId");
        }
    }
}
