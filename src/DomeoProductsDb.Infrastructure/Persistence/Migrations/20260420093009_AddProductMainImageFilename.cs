using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomeoProductsDb.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductMainImageFilename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainImageFilename",
                schema: "staging",
                table: "products",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainImageFilename",
                schema: "staging",
                table: "products");
        }
    }
}
