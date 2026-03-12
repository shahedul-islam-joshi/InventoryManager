using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventoryManager.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingFieldsToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemLikes_ItemId",
                table: "ItemLikes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Items",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "Bool1",
                table: "Items",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Bool2",
                table: "Items",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Bool3",
                table: "Items",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomId",
                table: "Items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date1",
                table: "Items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date2",
                table: "Items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date3",
                table: "Items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Number1",
                table: "Items",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Number2",
                table: "Items",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Number3",
                table: "Items",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text1",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text2",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text3",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Items",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Inventories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Inventories",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Inventories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Inventories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Inventories",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IdElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdElements_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SlotIndex = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ShowInTable = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryFields_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventorySequences",
                columns: table => new
                {
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentValue = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySequences", x => x.InventoryId);
                    table.ForeignKey(
                        name: "FK_InventorySequences_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTags",
                columns: table => new
                {
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTags", x => new { x.InventoryId, x.TagId });
                    table.ForeignKey(
                        name: "FK_InventoryTags_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryId",
                table: "Items",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryId_CustomId",
                table: "Items",
                columns: new[] { "InventoryId", "CustomId" },
                unique: true,
                filter: "\"CustomId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLikes_ItemId_UserId",
                table: "ItemLikes",
                columns: new[] { "ItemId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_OwnerId",
                table: "Inventories",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_IdElements_InventoryId",
                table: "IdElements",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryFields_InventoryId_FieldType_SlotIndex",
                table: "InventoryFields",
                columns: new[] { "InventoryId", "FieldType", "SlotIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTags_TagId",
                table: "InventoryTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_AspNetUsers_OwnerId",
                table: "Inventories",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAccesses_Inventories_InventoryId",
                table: "InventoryAccesses",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Inventories_InventoryId",
                table: "Items",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_AspNetUsers_OwnerId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAccesses_Inventories_InventoryId",
                table: "InventoryAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Inventories_InventoryId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "IdElements");

            migrationBuilder.DropTable(
                name: "InventoryFields");

            migrationBuilder.DropTable(
                name: "InventorySequences");

            migrationBuilder.DropTable(
                name: "InventoryTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Items_InventoryId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_InventoryId_CustomId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_ItemLikes_ItemId_UserId",
                table: "ItemLikes");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_OwnerId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Bool1",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Bool2",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Bool3",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Date1",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Date2",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Date3",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Number1",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Number2",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Number3",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Text1",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Text2",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Text3",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Inventories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Items",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Items",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Inventories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Inventories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Inventories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_ItemLikes_ItemId",
                table: "ItemLikes",
                column: "ItemId");
        }
    }
}
