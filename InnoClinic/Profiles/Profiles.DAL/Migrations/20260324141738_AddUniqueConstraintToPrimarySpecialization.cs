using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profiles.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToPrimarySpecialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StaffSpecialization_StaffId_IsPrimary",
                table: "StaffSpecialization",
                columns: new[] { "StaffId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffSpecialization_StaffId_IsPrimary",
                table: "StaffSpecialization");
        }
    }
}
