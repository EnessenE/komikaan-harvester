using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class ManyKeys3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_shapes",
                table: "shapes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shapes",
                table: "shapes",
                columns: new[] { "DataOrigin", "Id", "Sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_shapes",
                table: "shapes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shapes",
                table: "shapes",
                columns: new[] { "DataOrigin", "Id" });
        }
    }
}
