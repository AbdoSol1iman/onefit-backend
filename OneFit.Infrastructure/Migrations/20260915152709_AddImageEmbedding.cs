using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OneFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    brand_id = table.Column<string>(type: "character varying", nullable: false),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    application_user_id = table.Column<string>(type: "character varying", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("brands_pkey", x => x.brand_id);
                });

            migrationBuilder.CreateTable(
                name: "shoppers",
                columns: table => new
                {
                    shopper_id = table.Column<string>(type: "character varying", nullable: false),
                    name = table.Column<string>(type: "character varying", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("shoppers_pkey", x => x.shopper_id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    product_id = table.Column<string>(type: "character varying", nullable: false),
                    brand_id = table.Column<string>(type: "character varying", nullable: false),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    category = table.Column<string>(type: "character varying", nullable: false, comment: "shirt, pants, shoes, accessory, etc."),
                    price_egp = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    image_url = table.Column<string>(type: "character varying", nullable: true),
                    image_embedding = table.Column<string>(type: "vector(512)", nullable: true),
                    style_tags = table.Column<List<string>>(type: "character varying[]", nullable: true, comment: "e.g. linen, casual, chino — used by Catalog-Query Tool"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("products_pkey", x => x.product_id);
                    table.ForeignKey(
                        name: "fk_products_brand",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "brand_id");
                });

            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    cart_id = table.Column<string>(type: "character varying", nullable: false),
                    shopper_id = table.Column<string>(type: "character varying", nullable: false),
                    status = table.Column<string>(type: "character varying", nullable: true, defaultValueSql: "'active'::character varying", comment: "active, checked_out"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("carts_pkey", x => x.cart_id);
                    table.ForeignKey(
                        name: "fk_carts_shopper",
                        column: x => x.shopper_id,
                        principalTable: "shoppers",
                        principalColumn: "shopper_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    alert_id = table.Column<string>(type: "character varying", nullable: false),
                    shopper_id = table.Column<string>(type: "character varying", nullable: false),
                    alert_type = table.Column<string>(type: "character varying", nullable: false, comment: "PRICE_DROP or LOW_STOCK"),
                    product_id = table.Column<string>(type: "character varying", nullable: false),
                    size = table.Column<string>(type: "character varying", nullable: true, comment: "set only for LOW_STOCK alerts, per FR-26"),
                    old_price_egp = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    new_price_egp = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    percent_saved = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, comment: "FR-28"),
                    triggered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("alerts_pkey", x => x.alert_id);
                    table.ForeignKey(
                        name: "fk_alerts_product",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alerts_shopper",
                        column: x => x.shopper_id,
                        principalTable: "shoppers",
                        principalColumn: "shopper_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_sizes",
                columns: table => new
                {
                    product_id = table.Column<string>(type: "character varying", nullable: false),
                    size = table.Column<string>(type: "character varying", nullable: false, comment: "S, M, L, XL, etc."),
                    stock_qty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_sizes_pkey", x => new { x.product_id, x.size });
                    table.ForeignKey(
                        name: "fk_product_sizes_product",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "character varying", nullable: false),
                    shopper_id = table.Column<string>(type: "character varying", nullable: false),
                    cart_id = table.Column<string>(type: "character varying", nullable: false),
                    payment_status = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "'PAID_SIMULATED'::character varying", comment: "FR-23 — simulated only"),
                    grand_total_egp = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, comment: "must equal SUM(sub_orders.total_egp) — enforced in application code, not by the DB"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("orders_pkey", x => x.order_id);
                    table.ForeignKey(
                        name: "fk_orders_cart",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "cart_id");
                    table.ForeignKey(
                        name: "fk_orders_shopper",
                        column: x => x.shopper_id,
                        principalTable: "shoppers",
                        principalColumn: "shopper_id");
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    cart_item_id = table.Column<string>(type: "character varying", nullable: false),
                    cart_id = table.Column<string>(type: "character varying", nullable: false),
                    product_id = table.Column<string>(type: "character varying", nullable: false),
                    brand_id = table.Column<string>(type: "character varying", nullable: false, comment: "denormalized for FR-18 per-line-item brand display without a join"),
                    size = table.Column<string>(type: "character varying", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    price_at_add_egp = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, comment: "snapshot for FR-09/NFR-09 re-validation at checkout")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cart_items_pkey", x => x.cart_item_id);
                    table.ForeignKey(
                        name: "fk_cart_items_brand",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "brand_id");
                    table.ForeignKey(
                        name: "fk_cart_items_cart",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "cart_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cart_items_product_size",
                        columns: x => new { x.product_id, x.size },
                        principalTable: "product_sizes",
                        principalColumns: new[] { "product_id", "size" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wishlist_items",
                columns: table => new
                {
                    wishlist_item_id = table.Column<string>(type: "text", nullable: false),
                    shopper_id = table.Column<string>(type: "character varying", nullable: false),
                    product_id = table.Column<string>(type: "character varying", nullable: false),
                    size = table.Column<string>(type: "character varying", nullable: false),
                    price_at_save_egp = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlist_items", x => x.wishlist_item_id);
                    table.ForeignKey(
                        name: "FK_wishlist_items_product_sizes_product_id_size",
                        columns: x => new { x.product_id, x.size },
                        principalTable: "product_sizes",
                        principalColumns: new[] { "product_id", "size" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wishlist_items_shoppers_shopper_id",
                        column: x => x.shopper_id,
                        principalTable: "shoppers",
                        principalColumn: "shopper_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sub_orders",
                columns: table => new
                {
                    sub_order_id = table.Column<string>(type: "character varying", nullable: false),
                    order_id = table.Column<string>(type: "character varying", nullable: false),
                    brand_id = table.Column<string>(type: "character varying", nullable: false),
                    total_egp = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, comment: "FR-21 — one sub-order per brand; must equal SUM(order_items.price_at_purchase_egp * qty)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("sub_orders_pkey", x => x.sub_order_id);
                    table.ForeignKey(
                        name: "fk_sub_orders_brand",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "brand_id");
                    table.ForeignKey(
                        name: "fk_sub_orders_order",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    order_item_id = table.Column<string>(type: "character varying", nullable: false),
                    sub_order_id = table.Column<string>(type: "character varying", nullable: false),
                    product_id = table.Column<string>(type: "character varying", nullable: false),
                    size = table.Column<string>(type: "character varying", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    price_at_purchase_egp = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, comment: "final locked-in price at checkout — independent of later product.price_egp changes")
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_items_pkey", x => x.order_item_id);
                    table.ForeignKey(
                        name: "fk_order_items_product",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id");
                    table.ForeignKey(
                        name: "fk_order_items_sub_order",
                        column: x => x.sub_order_id,
                        principalTable: "sub_orders",
                        principalColumn: "sub_order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_alerts_shopper_unread",
                table: "alerts",
                column: "shopper_id",
                filter: "(is_read = false)");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_product_id",
                table: "alerts",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_brand_documents_brand_id",
                table: "brand_documents",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "idx_cart_items_cart_id",
                table: "cart_items",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_brand_id",
                table: "cart_items",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_product_id_size",
                table: "cart_items",
                columns: new[] { "product_id", "size" });

            migrationBuilder.CreateIndex(
                name: "uq_cart_item_product_size",
                table: "cart_items",
                columns: new[] { "cart_id", "product_id", "size" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carts_shopper_id",
                table: "carts",
                column: "shopper_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_items_sub_order_id",
                table: "order_items",
                column: "sub_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_shopper_id",
                table: "orders",
                column: "shopper_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_cart_id",
                table: "orders",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "idx_products_brand_id",
                table: "products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "idx_products_category",
                table: "products",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_products_style_tags",
                table: "products",
                column: "style_tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "idx_products_image_embedding",
                table: "products",
                column: "image_embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw");

            migrationBuilder.CreateIndex(
                name: "idx_sub_orders_brand_id",
                table: "sub_orders",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "idx_sub_orders_order_id",
                table: "sub_orders",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_wishlist_shopper_id",
                table: "wishlist_items",
                column: "shopper_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_items_product_id_size",
                table: "wishlist_items",
                columns: new[] { "product_id", "size" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "brand_documents");

            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "wishlist_items");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "sub_orders");

            migrationBuilder.DropTable(
                name: "product_sizes");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "carts");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "shoppers");
        }
    }
}
