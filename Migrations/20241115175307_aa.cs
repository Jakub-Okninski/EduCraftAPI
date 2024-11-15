using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCraftAPI.Migrations
{
    /// <inheritdoc />
    public partial class aa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Flashcards_FlashcardsID",
                table: "Flashcard");

            migrationBuilder.RenameColumn(
                name: "FlashcardsID",
                table: "Flashcard",
                newName: "FlashcardsID1");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcard_FlashcardsID",
                table: "Flashcard",
                newName: "IX_Flashcard_FlashcardsID1");

            migrationBuilder.AddColumn<string>(
                name: "FileContent",
                table: "Flashcard",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Flashcards_FlashcardsID1",
                table: "Flashcard",
                column: "FlashcardsID1",
                principalTable: "Flashcards",
                principalColumn: "FlashcardsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Flashcards_FlashcardsID1",
                table: "Flashcard");

            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Flashcard");

            migrationBuilder.RenameColumn(
                name: "FlashcardsID1",
                table: "Flashcard",
                newName: "FlashcardsID");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcard_FlashcardsID1",
                table: "Flashcard",
                newName: "IX_Flashcard_FlashcardsID");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Flashcards_FlashcardsID",
                table: "Flashcard",
                column: "FlashcardsID",
                principalTable: "Flashcards",
                principalColumn: "FlashcardsID");
        }
    }
}
