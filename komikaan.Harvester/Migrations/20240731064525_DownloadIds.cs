using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class DownloadIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "service_id",
                table: "trips",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "route_id",
                table: "trips",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "trips",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "to_stop_id",
                table: "transfers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "minimum_transfer_time",
                table: "transfers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "from_stop_id",
                table: "transfers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "transfers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "stops",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Point>(
                name: "geo_location",
                table: "stops",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry");

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "stops",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "stop_times",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Point>(
                name: "geo_location",
                table: "shapes",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry");

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "shapes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "short_name",
                table: "routes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "long_name",
                table: "routes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "agency_id",
                table: "routes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "routes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "to_stop_id",
                table: "pathway",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "signposted_as",
                table: "pathway",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "reversed_signposted_as",
                table: "pathway",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "from_stop_id",
                table: "pathway",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "pathway",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "headway_secs",
                table: "frequencies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "frequencies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "calenders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "calendar_dates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "agencies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "import_id",
                table: "agencies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_trips_import_id",
                table: "trips",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_service_id_data_origin",
                table: "trips",
                columns: new[] { "service_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_transfers_import_id",
                table: "transfers",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_stops_id_stop_type",
                table: "stops",
                columns: new[] { "id", "stop_type" });

            migrationBuilder.CreateIndex(
                name: "ix_stops_import_id",
                table: "stops",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_stops_internal_id_stop_type",
                table: "stops",
                columns: new[] { "internal_id", "stop_type" });

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_import_id",
                table: "stop_times",
                column: "import_id");

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
                name: "ix_shapes_import_id",
                table: "shapes",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_routes_import_id",
                table: "routes",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_pathway_import_id",
                table: "pathway",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_frequencies_import_id",
                table: "frequencies",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_calenders_import_id",
                table: "calenders",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_date",
                table: "calendar_dates",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_date_data_origin",
                table: "calendar_dates",
                columns: new[] { "date", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_import_id",
                table: "calendar_dates",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_service_id_data_origin",
                table: "calendar_dates",
                columns: new[] { "service_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_agencies_import_id",
                table: "agencies",
                column: "import_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_trips_import_id",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_trips_service_id_data_origin",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_transfers_import_id",
                table: "transfers");

            migrationBuilder.DropIndex(
                name: "ix_stops_id_stop_type",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stops_import_id",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stops_internal_id_stop_type",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_import_id",
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
                name: "ix_shapes_import_id",
                table: "shapes");

            migrationBuilder.DropIndex(
                name: "ix_routes_import_id",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "ix_pathway_import_id",
                table: "pathway");

            migrationBuilder.DropIndex(
                name: "ix_frequencies_import_id",
                table: "frequencies");

            migrationBuilder.DropIndex(
                name: "ix_calenders_import_id",
                table: "calenders");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_date",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_date_data_origin",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_import_id",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_service_id_data_origin",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_agencies_import_id",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "transfers");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "stops");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "stop_times");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "shapes");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "pathway");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "frequencies");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "calenders");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "calendar_dates");

            migrationBuilder.DropColumn(
                name: "import_id",
                table: "agencies");

            migrationBuilder.AlterColumn<string>(
                name: "service_id",
                table: "trips",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "route_id",
                table: "trips",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "to_stop_id",
                table: "transfers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "minimum_transfer_time",
                table: "transfers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "from_stop_id",
                table: "transfers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "stops",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Point>(
                name: "geo_location",
                table: "stops",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geometry",
                oldNullable: true);

            migrationBuilder.AlterColumn<Point>(
                name: "geo_location",
                table: "shapes",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geometry",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "short_name",
                table: "routes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "long_name",
                table: "routes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "agency_id",
                table: "routes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "to_stop_id",
                table: "pathway",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "signposted_as",
                table: "pathway",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "reversed_signposted_as",
                table: "pathway",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "from_stop_id",
                table: "pathway",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "headway_secs",
                table: "frequencies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "agencies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
