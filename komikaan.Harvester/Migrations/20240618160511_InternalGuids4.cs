using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class InternalGuids4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_trips",
                table: "trips");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transfers",
                table: "transfers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates");

            migrationBuilder.AddPrimaryKey(
                name: "pk_trips",
                table: "trips",
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_transfers",
                table: "transfers",
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates",
                columns: new[] { "internal_id", "data_origin", "date", "service_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_trips",
                table: "trips");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transfers",
                table: "transfers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates");

            migrationBuilder.AddPrimaryKey(
                name: "pk_trips",
                table: "trips",
                columns: new[] { "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_transfers",
                table: "transfers",
                columns: new[] { "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates",
                columns: new[] { "data_origin", "date", "service_id" });
        }
    }
}
