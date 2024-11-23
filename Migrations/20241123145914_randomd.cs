using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCraftAPI.Migrations
{
    /// <inheritdoc />
    public partial class randomd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Random",
                table: "Quiz",
                newName: "RandomQuestion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RandomQuestion",
                table: "Quiz",
                newName: "Random");
        }
    }
}
