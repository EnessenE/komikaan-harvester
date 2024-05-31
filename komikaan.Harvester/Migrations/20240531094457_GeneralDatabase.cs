using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class GeneralDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agencies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    URL = table.Column<string>(type: "text", nullable: true),
                    Timezone = table.Column<string>(type: "text", nullable: true),
                    LanguageCode = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    FareURL = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_dates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ServiceId = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExceptionType = table.Column<int>(type: "integer", nullable: false),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_dates", x => x.Id);
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
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calenders", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "frequencies",
                columns: table => new
                {
                    TripId = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<string>(type: "text", nullable: true),
                    EndTime = table.Column<string>(type: "text", nullable: true),
                    HeadwaySecs = table.Column<string>(type: "text", nullable: true),
                    ExactTimes = table.Column<bool>(type: "boolean", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AgencyId = table.Column<string>(type: "text", nullable: true),
                    ShortName = table.Column<string>(type: "text", nullable: true),
                    LongName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    TextColor = table.Column<string>(type: "text", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shapes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    DistanceTravelled = table.Column<double>(type: "double precision", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shapes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stop_times",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TripId = table.Column<string>(type: "text", nullable: true),
                    ArrivalTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    DepartureTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    StopId = table.Column<string>(type: "text", nullable: true),
                    StopSequence = table.Column<long>(type: "bigint", nullable: false),
                    StopHeadsign = table.Column<string>(type: "text", nullable: true),
                    PickupType = table.Column<int>(type: "integer", nullable: true),
                    DropOffType = table.Column<int>(type: "integer", nullable: true),
                    ShapeDistTravelled = table.Column<double>(type: "double precision", nullable: true),
                    TimepointType = table.Column<int>(type: "integer", nullable: false),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stop_times", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stops",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Zone = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    LocationType = table.Column<int>(type: "integer", nullable: true),
                    ParentStation = table.Column<string>(type: "text", nullable: true),
                    Timezone = table.Column<string>(type: "text", nullable: true),
                    WheelchairBoarding = table.Column<string>(type: "text", nullable: true),
                    LevelId = table.Column<string>(type: "text", nullable: true),
                    PlatformCode = table.Column<string>(type: "text", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transfers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FromStopId = table.Column<string>(type: "text", nullable: true),
                    ToStopId = table.Column<string>(type: "text", nullable: true),
                    TransferType = table.Column<int>(type: "integer", nullable: false),
                    MinimumTransferTime = table.Column<string>(type: "text", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfers", x => x.Id);
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
                    AccessibilityType = table.Column<int>(type: "integer", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agencies_DataOrigin",
                table: "agencies",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_agencies_Id_Name",
                table: "agencies",
                columns: new[] { "Id", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_dates_DataOrigin",
                table: "calendar_dates",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_calendar_dates_ServiceId",
                table: "calendar_dates",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_calenders_DataOrigin",
                table: "calenders",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_calenders_ServiceId",
                table: "calenders",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_frequencies_DataOrigin",
                table: "frequencies",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_routes_AgencyId",
                table: "routes",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_routes_DataOrigin",
                table: "routes",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_routes_LongName",
                table: "routes",
                column: "LongName");

            migrationBuilder.CreateIndex(
                name: "IX_routes_ShortName",
                table: "routes",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_shapes_DataOrigin",
                table: "shapes",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_stop_times_DataOrigin",
                table: "stop_times",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_stop_times_StopId",
                table: "stop_times",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_stop_times_TripId",
                table: "stop_times",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_stops_DataOrigin",
                table: "stops",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_stops_Name",
                table: "stops",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_stops_Name_ParentStation",
                table: "stops",
                columns: new[] { "Name", "ParentStation" });

            migrationBuilder.CreateIndex(
                name: "IX_stops_ParentStation",
                table: "stops",
                column: "ParentStation");

            migrationBuilder.CreateIndex(
                name: "IX_transfers_DataOrigin",
                table: "transfers",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_trips_DataOrigin",
                table: "trips",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_trips_RouteId",
                table: "trips",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_trips_ServiceId",
                table: "trips",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_trips_ShapeId",
                table: "trips",
                column: "ShapeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agencies");

            migrationBuilder.DropTable(
                name: "calendar_dates");

            migrationBuilder.DropTable(
                name: "calenders");

            migrationBuilder.DropTable(
                name: "frequencies");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "shapes");

            migrationBuilder.DropTable(
                name: "stop_times");

            migrationBuilder.DropTable(
                name: "stops");

            migrationBuilder.DropTable(
                name: "transfers");

            migrationBuilder.DropTable(
                name: "trips");
        }
    }
}
