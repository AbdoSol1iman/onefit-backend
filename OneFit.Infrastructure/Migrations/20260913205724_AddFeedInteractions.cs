using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_local",
                table: "brands",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "user_interactions",
                columns: table => new
                {
                    interaction_id = table.Column<string>(type: "character varying", nullable: false),
                    user_id = table.Column<string>(type: "character varying", nullable: false),
                    product_id = table.Column<string>(type: "character varying", nullable: false),
                    interaction_type = table.Column<string>(type: "character varying(20)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_interactions_pkey", x => x.interaction_id);
                    table.ForeignKey(
                        name: "fk_user_interactions_product",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_interactions_shopper",
                        column: x => x.user_id,
                        principalTable: "shoppers",
                        principalColumn: "shopper_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_user_interactions_user_id",
                table: "user_interactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_interactions_product_id",
                table: "user_interactions",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_interactions");

            migrationBuilder.DropColumn(
                name: "is_local",
                table: "brands");
        }
    }
}
