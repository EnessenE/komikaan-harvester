using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class LessPKs : Migration
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
                name: "pk_stops",
                table: "stops");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stop_times",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_trip_id_stop_id_data_origin",
                table: "stop_times");

            migrationBuilder.DropPrimaryKey(
                name: "pk_routes",
                table: "routes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_pathway",
                table: "pathway");

            migrationBuilder.DropPrimaryKey(
                name: "pk_frequencies",
                table: "frequencies");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calenders",
                table: "calenders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_agencies",
                table: "agencies");

            migrationBuilder.AddPrimaryKey(
                name: "pk_trips",
                table: "trips",
                columns: new[] { "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_transfers",
                table: "transfers",
                columns: new[] { "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_stops",
                table: "stops",
                columns: new[] { "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_stop_times",
                table: "stop_times",
                columns: new[] { "data_origin", "trip_id", "stop_id", "stop_sequence" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_routes",
                table: "routes",
                columns: new[] { "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_pathway",
                table: "pathway",
                columns: new[] { "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_frequencies",
                table: "frequencies",
                columns: new[] { "data_origin", "trip_id", "start_time", "end_time" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_calenders",
                table: "calenders",
                columns: new[] { "data_origin", "service_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates",
                columns: new[] { "data_origin", "date", "service_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_agencies",
                table: "agencies",
                columns: new[] { "data_origin", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_trip_id_stop_id_data_origin_stop_sequence",
                table: "stop_times",
                columns: new[] { "trip_id", "stop_id", "data_origin", "stop_sequence" });
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
                name: "pk_stops",
                table: "stops");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stop_times",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_trip_id_stop_id_data_origin_stop_sequence",
                table: "stop_times");

            migrationBuilder.DropPrimaryKey(
                name: "pk_routes",
                table: "routes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_pathway",
                table: "pathway");

            migrationBuilder.DropPrimaryKey(
                name: "pk_frequencies",
                table: "frequencies");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calenders",
                table: "calenders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_agencies",
                table: "agencies");

            migrationBuilder.AddPrimaryKey(
                name: "pk_trips",
                table: "trips",
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_transfers",
                table: "transfers",
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_stops",
                table: "stops",
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_stop_times",
                table: "stop_times",
                columns: new[] { "internal_id", "data_origin", "trip_id", "stop_id", "stop_sequence" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_routes",
                table: "routes",
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_pathway",
                table: "pathway",
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_frequencies",
                table: "frequencies",
                columns: new[] { "internal_id", "data_origin", "trip_id", "start_time", "end_time" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_calenders",
                table: "calenders",
                columns: new[] { "internal_id", "data_origin", "service_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates",
                columns: new[] { "internal_id", "data_origin", "date", "service_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_agencies",
                table: "agencies",
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_trip_id_stop_id_data_origin",
                table: "stop_times",
                columns: new[] { "trip_id", "stop_id", "data_origin" });
        }
    }
}
