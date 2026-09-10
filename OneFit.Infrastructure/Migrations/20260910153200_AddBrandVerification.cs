using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "application_user_id",
                table: "brands",
                type: "character varying",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "brands",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "brands",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "brand_documents",
                columns: table => new
                {
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_id = table.Column<string>(type: "character varying", nullable: false),
                    file_name = table.Column<string>(type: "character varying", nullable: false),
                    content_type = table.Column<string>(type: "character varying", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    storage_key = table.Column<string>(type: "character varying", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("brand_documents_pkey", x => x.document_id);
                    table.ForeignKey(
                        name: "brand_documents_brand_id_fkey",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "brand_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_brand_documents_brand_id",
                table: "brand_documents",
                column: "brand_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "brand_documents");

            migrationBuilder.DropColumn(
                name: "application_user_id",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "status",
                table: "brands");
        }
    }
}
