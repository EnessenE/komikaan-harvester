using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTooManyIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_trips_data_origin",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_transfers_data_origin",
                table: "transfers");

            migrationBuilder.DropIndex(
                name: "ix_stops_data_origin",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_arrival_time",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_data_origin",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_departure_time",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_stop_id_data_origin",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_trip_id_data_origin",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_trip_id_stop_sequence",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_shapes_data_origin",
                table: "shapes");

            migrationBuilder.DropIndex(
                name: "ix_routes_data_origin",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "ix_pathway_data_origin",
                table: "pathway");

            migrationBuilder.DropIndex(
                name: "ix_frequencies_data_origin",
                table: "frequencies");

            migrationBuilder.DropIndex(
                name: "ix_calenders_data_origin",
                table: "calenders");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_data_origin",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_agencies_data_origin",
                table: "agencies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_trips_data_origin",
                table: "trips",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_transfers_data_origin",
                table: "transfers",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_stops_data_origin",
                table: "stops",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_arrival_time",
                table: "stop_times",
                column: "arrival_time");

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_data_origin",
                table: "stop_times",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_departure_time",
                table: "stop_times",
                column: "departure_time");

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_stop_id_data_origin",
                table: "stop_times",
                columns: new[] { "stop_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_trip_id_data_origin",
                table: "stop_times",
                columns: new[] { "trip_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_trip_id_stop_sequence",
                table: "stop_times",
                columns: new[] { "trip_id", "stop_sequence" });

            migrationBuilder.CreateIndex(
                name: "ix_shapes_data_origin",
                table: "shapes",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_routes_data_origin",
                table: "routes",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_pathway_data_origin",
                table: "pathway",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_frequencies_data_origin",
                table: "frequencies",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_calenders_data_origin",
                table: "calenders",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_data_origin",
                table: "calendar_dates",
                column: "data_origin");

            migrationBuilder.CreateIndex(
                name: "ix_agencies_data_origin",
                table: "agencies",
                column: "data_origin");
        }
    }
}
