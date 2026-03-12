using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManager.Migrations
{
    /// <inheritdoc />
    public partial class RestrictInventoryOwnerDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Patch existing NULL OwnerIds by assigning them to the default admin
            migrationBuilder.Sql(@"
                UPDATE ""Inventories"" 
                SET ""OwnerId"" = (SELECT ""Id"" FROM ""AspNetUsers"" WHERE ""Email"" = 'admin@admin.com')
                WHERE ""OwnerId"" IS NULL;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_AspNetUsers_OwnerId",
                table: "Inventories");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_AspNetUsers_OwnerId",
                table: "Inventories",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_AspNetUsers_OwnerId",
                table: "Inventories");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_AspNetUsers_OwnerId",
                table: "Inventories",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
