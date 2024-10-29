using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCraftAPI.Migrations
{
    /// <inheritdoc />
    public partial class quizUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Quiz",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_UserID",
                table: "Quiz",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_User_UserID",
                table: "Quiz",
                column: "UserID",
                principalTable: "User",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_User_UserID",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_UserID",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Quiz");
        }
    }
}
