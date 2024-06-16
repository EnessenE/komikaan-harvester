using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class BetterIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_trips_id",
                table: "trips",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_id_data_origin",
                table: "trips",
                columns: new[] { "id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_internal_id",
                table: "trips",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_internal_id_data_origin",
                table: "trips",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_transfers_internal_id",
                table: "transfers",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_transfers_internal_id_data_origin",
                table: "transfers",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_stops_id_data_origin",
                table: "stops",
                columns: new[] { "id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_stops_internal_id_data_origin",
                table: "stops",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_internal_id",
                table: "stop_times",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_internal_id_data_origin",
                table: "stop_times",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_trip_id_stop_id_data_origin",
                table: "stop_times",
                columns: new[] { "trip_id", "stop_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_shapes_id_data_origin_sequence",
                table: "shapes",
                columns: new[] { "id", "data_origin", "sequence" });

            migrationBuilder.CreateIndex(
                name: "ix_shapes_internal_id",
                table: "shapes",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_shapes_internal_id_data_origin",
                table: "shapes",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_routes_id",
                table: "routes",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_routes_id_data_origin",
                table: "routes",
                columns: new[] { "id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_routes_internal_id",
                table: "routes",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_routes_internal_id_data_origin",
                table: "routes",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_pathway_internal_id",
                table: "pathway",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_pathway_internal_id_data_origin",
                table: "pathway",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_frequencies_internal_id",
                table: "frequencies",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_frequencies_internal_id_data_origin",
                table: "frequencies",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_frequencies_trip_id_data_origin_start_time_end_time",
                table: "frequencies",
                columns: new[] { "trip_id", "data_origin", "start_time", "end_time" });

            migrationBuilder.CreateIndex(
                name: "ix_calenders_internal_id",
                table: "calenders",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_calenders_internal_id_data_origin",
                table: "calenders",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_calenders_service_id_data_origin",
                table: "calenders",
                columns: new[] { "service_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_internal_id",
                table: "calendar_dates",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_internal_id_data_origin",
                table: "calendar_dates",
                columns: new[] { "internal_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_service_id_date_data_origin",
                table: "calendar_dates",
                columns: new[] { "service_id", "date", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_agencies_id",
                table: "agencies",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_agencies_id_data_origin",
                table: "agencies",
                columns: new[] { "id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_agencies_internal_id",
                table: "agencies",
                column: "internal_id");

            migrationBuilder.CreateIndex(
                name: "ix_agencies_internal_id_data_origin",
                table: "agencies",
                columns: new[] { "internal_id", "data_origin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_trips_id",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_trips_id_data_origin",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_trips_internal_id",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_trips_internal_id_data_origin",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_transfers_internal_id",
                table: "transfers");

            migrationBuilder.DropIndex(
                name: "ix_transfers_internal_id_data_origin",
                table: "transfers");

            migrationBuilder.DropIndex(
                name: "ix_stops_id_data_origin",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stops_internal_id_data_origin",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_internal_id",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_internal_id_data_origin",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_trip_id_stop_id_data_origin",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_shapes_id_data_origin_sequence",
                table: "shapes");

            migrationBuilder.DropIndex(
                name: "ix_shapes_internal_id",
                table: "shapes");

            migrationBuilder.DropIndex(
                name: "ix_shapes_internal_id_data_origin",
                table: "shapes");

            migrationBuilder.DropIndex(
                name: "ix_routes_id",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "ix_routes_id_data_origin",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "ix_routes_internal_id",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "ix_routes_internal_id_data_origin",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "ix_pathway_internal_id",
                table: "pathway");

            migrationBuilder.DropIndex(
                name: "ix_pathway_internal_id_data_origin",
                table: "pathway");

            migrationBuilder.DropIndex(
                name: "ix_frequencies_internal_id",
                table: "frequencies");

            migrationBuilder.DropIndex(
                name: "ix_frequencies_internal_id_data_origin",
                table: "frequencies");

            migrationBuilder.DropIndex(
                name: "ix_frequencies_trip_id_data_origin_start_time_end_time",
                table: "frequencies");

            migrationBuilder.DropIndex(
                name: "ix_calenders_internal_id",
                table: "calenders");

            migrationBuilder.DropIndex(
                name: "ix_calenders_internal_id_data_origin",
                table: "calenders");

            migrationBuilder.DropIndex(
                name: "ix_calenders_service_id_data_origin",
                table: "calenders");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_internal_id",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_internal_id_data_origin",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_service_id_date_data_origin",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_agencies_id",
                table: "agencies");

            migrationBuilder.DropIndex(
                name: "ix_agencies_id_data_origin",
                table: "agencies");

            migrationBuilder.DropIndex(
                name: "ix_agencies_internal_id",
                table: "agencies");

            migrationBuilder.DropIndex(
                name: "ix_agencies_internal_id_data_origin",
                table: "agencies");
        }
    }
}
