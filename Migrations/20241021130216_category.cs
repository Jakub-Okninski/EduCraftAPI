using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCraftAPI.Migrations
{
    /// <inheritdoc />
    public partial class category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryID",
                table: "Presentation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Presentation",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Presentation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Presentation_CategoryID",
                table: "Presentation",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Presentation_Category_CategoryID",
                table: "Presentation",
                column: "CategoryID",
                principalTable: "Category",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presentation_Category_CategoryID",
                table: "Presentation");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Presentation_CategoryID",
                table: "Presentation");

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "Presentation");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Presentation");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Presentation");
        }
    }
}
