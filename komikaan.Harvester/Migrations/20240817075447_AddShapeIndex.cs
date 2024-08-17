using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace komikaan.Harvester.Migrations
{
    /// <inheritdoc />
    public partial class AddShapeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_shapes_id_data_origin_sequence",
                table: "shapes",
                columns: new[] { "id", "data_origin", "sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_shapes_id_data_origin_sequence",
                table: "shapes");
        }
    }
}
