using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class InternalGuids3 : Migration
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

            migrationBuilder.DropPrimaryKey(
                name: "pk_shapes",
                table: "shapes");

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

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "trips",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "transfers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "stops",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "shapes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "routes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "pathway",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "agencies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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
                columns: new[] { "internal_id", "data_origin", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_stop_times",
                table: "stop_times",
                columns: new[] { "internal_id", "data_origin", "trip_id", "stop_id", "stop_sequence" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_shapes",
                table: "shapes",
                columns: new[] { "internal_id", "data_origin", "id", "sequence" });

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
                columns: new[] { "data_origin", "date", "service_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_agencies",
                table: "agencies",
                columns: new[] { "internal_id", "data_origin", "id" });
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

            migrationBuilder.DropPrimaryKey(
                name: "pk_shapes",
                table: "shapes");

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

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "trips",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "transfers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "stops",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "shapes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "routes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "pathway",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "agencies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "pk_trips",
                table: "trips",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transfers",
                table: "transfers",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stops",
                table: "stops",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stop_times",
                table: "stop_times",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shapes",
                table: "shapes",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_routes",
                table: "routes",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pathway",
                table: "pathway",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_frequencies",
                table: "frequencies",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_calenders",
                table: "calenders",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_calendar_dates",
                table: "calendar_dates",
                column: "internal_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_agencies",
                table: "agencies",
                column: "internal_id");
        }
    }
}
