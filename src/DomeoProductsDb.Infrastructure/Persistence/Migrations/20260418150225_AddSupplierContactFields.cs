using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomeoProductsDb.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "reference",
                table: "suppliers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "reference",
                table: "suppliers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "reference",
                table: "suppliers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Inn",
                schema: "reference",
                table: "suppliers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "reference",
                table: "suppliers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "reference",
                table: "suppliers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "reference",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "reference",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "reference",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Inn",
                schema: "reference",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "reference",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "reference",
                table: "suppliers");
        }
    }
}
