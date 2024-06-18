using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class InternalGuidsNotText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_stops",
                table: "stops");

            migrationBuilder.AlterColumn<Guid>(
                name: "internal_id",
                table: "stops",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "stops",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stops",
                table: "stops",
                column: "internal_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_stops",
                table: "stops");

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
                name: "internal_id",
                table: "stops",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stops",
                table: "stops",
                column: "id");
        }
    }
}
