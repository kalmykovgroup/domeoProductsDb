using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DomeoProductsDb.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reference");

            migrationBuilder.EnsureSchema(
                name: "staging");

            migrationBuilder.CreateTable(
                name: "attributes",
                schema: "reference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleRu = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                schema: "reference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    TitleRu = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "reference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleRu = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsLeaf = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_categories_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "reference",
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                schema: "reference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "enum_values",
                schema: "reference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleRu = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_enum_values_attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalSchema: "reference",
                        principalTable: "attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ExternalCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "reference",
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_attribute_values",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_id = table.Column<int>(type: "integer", nullable: false),
                    value_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    value_text = table.Column<string>(type: "text", nullable: true),
                    value_numeric = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    value_bool = table.Column<bool>(type: "boolean", nullable: true),
                    enum_value_id = table.Column<int>(type: "integer", nullable: true),
                    brand_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_attribute_values", x => x.Id);
                    table.CheckConstraint("ck_pav_type_exclusive", "(value_type = 'Text'    AND value_text IS NOT NULL AND value_numeric IS NULL AND value_bool IS NULL AND enum_value_id IS NULL AND brand_id IS NULL) OR (value_type = 'Numeric' AND value_numeric IS NOT NULL AND value_text IS NULL AND value_bool IS NULL AND enum_value_id IS NULL AND brand_id IS NULL) OR (value_type = 'Bool'    AND value_bool IS NOT NULL AND value_text IS NULL AND value_numeric IS NULL AND enum_value_id IS NULL AND brand_id IS NULL) OR (value_type = 'Enum'    AND enum_value_id IS NOT NULL AND value_text IS NULL AND value_numeric IS NULL AND value_bool IS NULL AND brand_id IS NULL) OR (value_type = 'Brand'   AND brand_id IS NOT NULL AND value_text IS NULL AND value_numeric IS NULL AND value_bool IS NULL AND enum_value_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_product_attribute_values_attributes_attribute_id",
                        column: x => x.attribute_id,
                        principalSchema: "reference",
                        principalTable: "attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_attribute_values_brands_brand_id",
                        column: x => x.brand_id,
                        principalSchema: "reference",
                        principalTable: "brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_attribute_values_enum_values_enum_value_id",
                        column: x => x.enum_value_id,
                        principalSchema: "reference",
                        principalTable: "enum_values",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_attribute_values_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "staging",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_offers",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_offers_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "staging",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplier_offers_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "reference",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attributes_Code",
                schema: "reference",
                table: "attributes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_brands_TitleRu",
                schema: "reference",
                table: "brands",
                column: "TitleRu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_Code",
                schema: "reference",
                table: "categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_ParentId",
                schema: "reference",
                table: "categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_enum_values_AttributeId_Code",
                schema: "reference",
                table: "enum_values",
                columns: new[] { "AttributeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_attribute_values_attribute_id",
                schema: "staging",
                table: "product_attribute_values",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_attribute_values_brand_id",
                schema: "staging",
                table: "product_attribute_values",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_attribute_values_enum_value_id",
                schema: "staging",
                table: "product_attribute_values",
                column: "enum_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_attribute_values_product_id_attribute_id",
                schema: "staging",
                table: "product_attribute_values",
                columns: new[] { "product_id", "attribute_id" });

            migrationBuilder.CreateIndex(
                name: "IX_products_CategoryId",
                schema: "staging",
                table: "products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_products_ExternalCode",
                schema: "staging",
                table: "products",
                column: "ExternalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_offers_ProductId",
                schema: "staging",
                table: "supplier_offers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_offers_SupplierId",
                schema: "staging",
                table: "supplier_offers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_Name",
                schema: "reference",
                table: "suppliers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_attribute_values",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "supplier_offers",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "brands",
                schema: "reference");

            migrationBuilder.DropTable(
                name: "enum_values",
                schema: "reference");

            migrationBuilder.DropTable(
                name: "products",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "suppliers",
                schema: "reference");

            migrationBuilder.DropTable(
                name: "attributes",
                schema: "reference");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "reference");
        }
    }
}
