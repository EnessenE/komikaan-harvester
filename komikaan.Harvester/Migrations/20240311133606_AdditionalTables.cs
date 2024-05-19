using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calendar_dates",
                columns: table => new
                {
                    ServiceId = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExceptionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_dates", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "calenders",
                columns: table => new
                {
                    ServiceId = table.Column<string>(type: "text", nullable: false),
                    Mask = table.Column<byte>(type: "smallint", nullable: false),
                    Monday = table.Column<bool>(type: "boolean", nullable: false),
                    Tuesday = table.Column<bool>(type: "boolean", nullable: false),
                    Wednesday = table.Column<bool>(type: "boolean", nullable: false),
                    Thursday = table.Column<bool>(type: "boolean", nullable: false),
                    Friday = table.Column<bool>(type: "boolean", nullable: false),
                    Saturday = table.Column<bool>(type: "boolean", nullable: false),
                    Sunday = table.Column<bool>(type: "boolean", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calenders", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "frequencies",
                columns: table => new
                {
                    TripId = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<string>(type: "text", nullable: true),
                    EndTime = table.Column<string>(type: "text", nullable: true),
                    HeadwaySecs = table.Column<string>(type: "text", nullable: true),
                    ExactTimes = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frequencies", x => x.TripId);
                });

            migrationBuilder.CreateTable(
                name: "stop_times",
                columns: table => new
                {
                    TripId = table.Column<string>(type: "text", nullable: false),
                    ArrivalTime = table.Column<string>(type: "text", nullable: true),
                    DepartureTime = table.Column<string>(type: "text", nullable: true),
                    StopId = table.Column<string>(type: "text", nullable: true),
                    StopSequence = table.Column<long>(type: "bigint", nullable: false),
                    StopHeadsign = table.Column<string>(type: "text", nullable: true),
                    PickupType = table.Column<int>(type: "integer", nullable: true),
                    DropOffType = table.Column<int>(type: "integer", nullable: true),
                    ShapeDistTravelled = table.Column<double>(type: "double precision", nullable: true),
                    TimepointType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stop_times", x => x.TripId);
                });

            migrationBuilder.CreateTable(
                name: "transfers",
                columns: table => new
                {
                    FromStopId = table.Column<string>(type: "text", nullable: true),
                    ToStopId = table.Column<string>(type: "text", nullable: true),
                    TransferType = table.Column<int>(type: "integer", nullable: false),
                    MinimumTransferTime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RouteId = table.Column<string>(type: "text", nullable: true),
                    ServiceId = table.Column<string>(type: "text", nullable: true),
                    Headsign = table.Column<string>(type: "text", nullable: true),
                    ShortName = table.Column<string>(type: "text", nullable: true),
                    Direction = table.Column<int>(type: "integer", nullable: true),
                    BlockId = table.Column<string>(type: "text", nullable: true),
                    ShapeId = table.Column<string>(type: "text", nullable: true),
                    AccessibilityType = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_dates");

            migrationBuilder.DropTable(
                name: "calenders");

            migrationBuilder.DropTable(
                name: "frequencies");

            migrationBuilder.DropTable(
                name: "stop_times");

            migrationBuilder.DropTable(
                name: "transfers");

            migrationBuilder.DropTable(
                name: "trips");
        }
    }
}
