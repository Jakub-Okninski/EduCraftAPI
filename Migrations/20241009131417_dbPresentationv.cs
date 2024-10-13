using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCraftAPI.Migrations
{
    /// <inheritdoc />
    public partial class dbPresentationv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PresentationID",
                table: "Presentation",
                newName: "PresentationsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PresentationsID",
                table: "Presentation",
                newName: "PresentationID");
        }
    }
}
