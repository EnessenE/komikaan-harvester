using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "trips",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "transfers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "stops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "stop_times",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "shapes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "routes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "frequencies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "calenders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "calendar_dates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                table: "agencies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_trips_DataOrigin",
                table: "trips",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_transfers_DataOrigin",
                table: "transfers",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_stops_DataOrigin",
                table: "stops",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_stop_times_DataOrigin",
                table: "stop_times",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_shapes_DataOrigin",
                table: "shapes",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_routes_DataOrigin",
                table: "routes",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_frequencies_DataOrigin",
                table: "frequencies",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_calenders_DataOrigin",
                table: "calenders",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_calendar_dates_DataOrigin",
                table: "calendar_dates",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_agencies_DataOrigin",
                table: "agencies",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_agencies_Id_Name",
                table: "agencies",
                columns: new[] { "Id", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trips_DataOrigin",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "IX_transfers_DataOrigin",
                table: "transfers");

            migrationBuilder.DropIndex(
                name: "IX_stops_DataOrigin",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "IX_stop_times_DataOrigin",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "IX_shapes_DataOrigin",
                table: "shapes");

            migrationBuilder.DropIndex(
                name: "IX_routes_DataOrigin",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "IX_frequencies_DataOrigin",
                table: "frequencies");

            migrationBuilder.DropIndex(
                name: "IX_calenders_DataOrigin",
                table: "calenders");

            migrationBuilder.DropIndex(
                name: "IX_calendar_dates_DataOrigin",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "IX_agencies_DataOrigin",
                table: "agencies");

            migrationBuilder.DropIndex(
                name: "IX_agencies_Id_Name",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "transfers");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "stops");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "stop_times");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "shapes");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "frequencies");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "calenders");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "calendar_dates");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                table: "agencies");
        }
    }
}
