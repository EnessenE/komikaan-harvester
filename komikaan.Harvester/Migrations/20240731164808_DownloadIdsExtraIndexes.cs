using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class DownloadIdsExtraIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_trips_import_id",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_transfers_import_id",
                table: "transfers");

            migrationBuilder.DropIndex(
                name: "ix_stops_import_id",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_import_id",
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
                name: "ix_calendar_dates_import_id",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_agencies_import_id",
                table: "agencies");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "trips",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "transfers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "stops",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "stop_times",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "shapes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "routes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "pathway",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "frequencies",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "calenders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "calendar_dates",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "agencies",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "ix_trips_import_id_data_origin",
                table: "trips",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_transfers_import_id_data_origin",
                table: "transfers",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_stops_import_id_data_origin",
                table: "stops",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_import_id_data_origin",
                table: "stop_times",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_shapes_import_id_data_origin",
                table: "shapes",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_routes_import_id_data_origin",
                table: "routes",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_pathway_import_id_data_origin",
                table: "pathway",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_frequencies_import_id_data_origin",
                table: "frequencies",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_calenders_import_id_data_origin",
                table: "calenders",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_dates_import_id_data_origin",
                table: "calendar_dates",
                columns: new[] { "import_id", "data_origin" });

            migrationBuilder.CreateIndex(
                name: "ix_agencies_import_id_data_origin",
                table: "agencies",
                columns: new[] { "import_id", "data_origin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_trips_import_id_data_origin",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_transfers_import_id_data_origin",
                table: "transfers");

            migrationBuilder.DropIndex(
                name: "ix_stops_import_id_data_origin",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stop_times_import_id_data_origin",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "ix_shapes_import_id_data_origin",
                table: "shapes");

            migrationBuilder.DropIndex(
                name: "ix_routes_import_id_data_origin",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "ix_pathway_import_id_data_origin",
                table: "pathway");

            migrationBuilder.DropIndex(
                name: "ix_frequencies_import_id_data_origin",
                table: "frequencies");

            migrationBuilder.DropIndex(
                name: "ix_calenders_import_id_data_origin",
                table: "calenders");

            migrationBuilder.DropIndex(
                name: "ix_calendar_dates_import_id_data_origin",
                table: "calendar_dates");

            migrationBuilder.DropIndex(
                name: "ix_agencies_import_id_data_origin",
                table: "agencies");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "trips",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "transfers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "stops",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "stop_times",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "shapes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "routes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "pathway",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "frequencies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "calenders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "calendar_dates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_id",
                table: "agencies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_trips_import_id",
                table: "trips",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_transfers_import_id",
                table: "transfers",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_stops_import_id",
                table: "stops",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_stop_times_import_id",
                table: "stop_times",
                column: "import_id");

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
                name: "ix_calendar_dates_import_id",
                table: "calendar_dates",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_agencies_import_id",
                table: "agencies",
                column: "import_id");
        }
    }
}
