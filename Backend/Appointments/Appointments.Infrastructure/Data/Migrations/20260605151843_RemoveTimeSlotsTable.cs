using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointments.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTimeSlotsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeSlots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MedicalStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                });
        }
    }
}
