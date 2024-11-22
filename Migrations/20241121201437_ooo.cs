using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCraftAPI.Migrations
{
    /// <inheritdoc />
    public partial class ooo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryID",
                table: "Quiz",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Quiz",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CategoryID",
                table: "Flashcards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Flashcards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_CategoryID",
                table: "Quiz",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_CategoryID",
                table: "Flashcards",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Category_CategoryID",
                table: "Flashcards",
                column: "CategoryID",
                principalTable: "Category",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_Category_CategoryID",
                table: "Quiz",
                column: "CategoryID",
                principalTable: "Category",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Category_CategoryID",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_Category_CategoryID",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_CategoryID",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_CategoryID",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Flashcards");
        }
    }
}
