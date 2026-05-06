using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profiles.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkingHoursAndOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleOverrides",
                columns: table => new
                {
                    MedicalStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsDayOff = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleOverrides", x => new { x.MedicalStaffId, x.Date });
                    table.ForeignKey(
                        name: "FK_ScheduleOverrides_Staff_MedicalStaffId",
                        column: x => x.MedicalStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkingHours",
                columns: table => new
                {
                    MedicalStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsDayOff = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingHours", x => new { x.MedicalStaffId, x.DayOfWeek });
                    table.ForeignKey(
                        name: "FK_WorkingHours_Staff_MedicalStaffId",
                        column: x => x.MedicalStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleOverrides");

            migrationBuilder.DropTable(
                name: "WorkingHours");
        }
    }
}
