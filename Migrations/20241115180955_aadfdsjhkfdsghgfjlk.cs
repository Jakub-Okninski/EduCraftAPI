using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCraftAPI.Migrations
{
    /// <inheritdoc />
    public partial class aadfdsjhkfdsghgfjlk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Flashcards_FlashcardsID",
                table: "Flashcard");

            migrationBuilder.AlterColumn<int>(
                name: "FlashcardsID",
                table: "Flashcard",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Flashcards_FlashcardsID",
                table: "Flashcard",
                column: "FlashcardsID",
                principalTable: "Flashcards",
                principalColumn: "FlashcardsID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Flashcards_FlashcardsID",
                table: "Flashcard");

            migrationBuilder.AlterColumn<int>(
                name: "FlashcardsID",
                table: "Flashcard",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Flashcards_FlashcardsID",
                table: "Flashcard",
                column: "FlashcardsID",
                principalTable: "Flashcards",
                principalColumn: "FlashcardsID");
        }
    }
}
